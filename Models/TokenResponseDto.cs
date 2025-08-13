namespace SawirahMunicipalityWeb.Models
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public  string? RefreshToken { get; set; }
        public string? FullName { get; set; }

        public string? Role { get; set; }
        public string? email { get; set; }
        public string? ProfilePhoto { get; set; }
    }
}
