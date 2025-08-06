using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class CreateNewsItemDto
    {
        public string? ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public Visibility Visibility { get; set; }
    }
}
