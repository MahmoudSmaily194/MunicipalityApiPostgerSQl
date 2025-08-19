
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.AuhtServices;
using System.Security.Claims;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]

        public async Task<IActionResult> Register(RegisterDto request)
        {
            var result = await authService.RegisterAsync(request);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }
            return Ok("User registered successfully");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {

            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token is missing");

            var result = await authService.RefreshTokenAsync();
            if (result is null)
                return Unauthorized("Invalid refresh token");

            return Ok(result);

        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPut("update_profile_photo")]

        public async Task<IActionResult> UpdateProfilePhoto([FromForm] UpdateProfileImageDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim.Value);
            try
            {
                var imageUrl = await authService.UpdateProfileImageAsync(userId, request.File);
                return Ok(new { message = "Profile image updated successfully", imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
