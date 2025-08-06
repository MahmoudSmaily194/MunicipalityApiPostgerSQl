namespace SawirahMunicipalityWeb.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;

        // To support token rotation & reuse detection
        public string? ReplacedByToken { get; set; }
         
        // Optional: track client info for security auditing
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }

        // Relation to user
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
