using Microsoft.AspNetCore.Identity;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.AuhtServices
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<IdentityResult> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto?> RefreshTokenAsync();
        Task LogoutAsync();
        Task<string> UpdateProfileImageAsync(Guid userId, IFormFile file);

    }
}
