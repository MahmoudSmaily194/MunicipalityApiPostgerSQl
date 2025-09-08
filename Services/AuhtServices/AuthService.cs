using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Enums;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.ImageService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SawirahMunicipalityWeb.Services.AuhtServices
{
    public class AuthService : IAuthService
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly SupabaseImageService _imageService;

        public AuthService(
            DBContext context,
            IConfiguration configuration,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            SupabaseImageService imageService)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _imageService = imageService;
        }

        // ================= Login =================
        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            bool isEmail = request.EmailOrPhone.Contains("@");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => isEmail
                    ? u.Email == request.EmailOrPhone
                    : u.PhoneNumber == request.EmailOrPhone);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return null;

            var accessToken = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            SetRefreshTokenCookie(refreshToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                email = user.Email,
                ProfilePhoto = user.ProfilePhoto,
                RefreshToken = null // Refresh token stored in HttpOnly cookie
            };
        }

        // ================= Register =================
        public async Task<IdentityResult> RegisterAsync(RegisterDto request)
        {
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.PhoneNumber == request.PhoneNumber);

            if (existingUser != null)
                return IdentityResult.Failed(new IdentityError { Description = "Email or phone already exists." });

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = Enum.TryParse<Roles>(request.Role, true, out var parsedRole) ? parsedRole : Roles.User
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return result;

            if (!await _roleManager.RoleExistsAsync(user.Role.ToString()))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(user.Role.ToString()));

            await _userManager.AddToRoleAsync(user, user.Role.ToString());
            return result;
        }

        // ================== Token Generation ==================
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var token = GenerateRefreshToken();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var refreshToken = new RefreshToken
            {
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(35), // ✅ 35-day expiry
                UserId = user.Id,
                CreatedByIp = ipAddress,
                UserAgent = userAgent,
                IsUsed = false,
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return token;
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(35)
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", refreshToken, options);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
            };

            var keyString = _configuration.GetValue<string>("AppSettings:Token") ?? throw new Exception("JWT key missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var issuer = _configuration.GetValue<string>("AppSettings:Issuer");
            var audience = _configuration.GetValue<string>("AppSettings:Audience");

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        // ================= Refresh Token =================
        public async Task<TokenResponseDto?> RefreshTokenAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var storedToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken == null) return null;
            if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow) return null;

            // ✅ Rotation: generate new token first
            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(storedToken.User);

            // Mark old token as used and store replacedBy
            storedToken.IsUsed = true;
            storedToken.ReplacedByToken = newRefreshToken;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            SetRefreshTokenCookie(newRefreshToken);

            var accessToken = CreateToken(storedToken.User);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                FullName = storedToken.User.FullName,
                Role = storedToken.User.Role.ToString(),
                email = storedToken.User.Email,
                ProfilePhoto = storedToken.User.ProfilePhoto,
                RefreshToken = null
            };
        }

        // ================= Logout =================
        public async Task LogoutAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return;

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
        }

        // ================= Profile Image =================
        public async Task<string> UpdateProfileImageAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only JPG and PNG are allowed.");

            string imageUrl = await _imageService.UploadImageAsync(file, "sawirah-images");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            user.ProfilePhoto = imageUrl;
            await _context.SaveChangesAsync();

            return user.ProfilePhoto;
        }
    }
}
