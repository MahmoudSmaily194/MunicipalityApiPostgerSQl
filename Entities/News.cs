using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Entities
{
    public class News
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Visibility Visibility { get; set; } 
       public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Slug { get; set; } = string.Empty;

    }
}
