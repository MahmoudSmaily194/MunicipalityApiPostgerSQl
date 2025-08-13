
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Enums;
using SawirahMunicipalityWeb.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

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
        public AuthService(DBContext context,
            IConfiguration configuration,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment)

        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            bool isEmail = request.EmailOrPhone.Contains("@");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => isEmail
                    ? u.Email == request.EmailOrPhone
                    : u.PhoneNumber == request.EmailOrPhone);

            if (user == null)
                return null;

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return null;

            var accessToken = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", refreshToken, refreshTokenCookieOptions);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                RefreshToken = null,
                email = user.Email,
                ProfilePhoto = user.ProfilePhoto
            };
        }

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
            if (!result.Succeeded)
                return result;

            if (!await _roleManager.RoleExistsAsync(user.Role.ToString()))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(user.Role.ToString()));

            await _userManager.AddToRoleAsync(user, user.Role.ToString());

            return result;
        }

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
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return token;
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

            var keyString = _configuration.GetValue<string>("AppSettings:Token");
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT signing key is not configured.");

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

        public async Task<TokenResponseDto?> RefreshTokenAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var storedToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken == null)
                return null;

            // ❌ Expired token
            if (storedToken.ExpiresAt < DateTime.UtcNow)
                return null;

            // ❌ Token reuse or revoked
            if (storedToken.IsUsed || storedToken.IsRevoked)
            {
                var userTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == storedToken.UserId && !t.IsRevoked)
                    .ToListAsync();

                foreach (var token in userTokens)
                    token.IsRevoked = true;

                await _context.SaveChangesAsync();
                return null;
            }

            // ❌ Check maximum refresh window
            if ((DateTime.UtcNow - storedToken.CreatedAt).TotalDays > 30)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
                return null;
            }

            storedToken.IsUsed = true;

            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(storedToken.User);
            storedToken.ReplacedByToken = newRefreshToken;

            var accessToken = CreateToken(storedToken.User);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(35)
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", newRefreshToken, refreshTokenCookieOptions);

            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                FullName = storedToken.User.FullName,
                Role = storedToken.User.Role.ToString(),
                email = storedToken.User.Email,
                RefreshToken = null,
                ProfilePhoto=storedToken.User.ProfilePhoto
            };
        }
        public async Task LogoutAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            // حذف الكوكي من المتصفح
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
        }
        public async Task<string> UpdateProfileImageAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only JPG and PNG are allowed.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            user.ProfilePhoto = $"/uploads/{uniqueFileName}";

            await _context.SaveChangesAsync();

            return user.ProfilePhoto;
        }


    }
}
