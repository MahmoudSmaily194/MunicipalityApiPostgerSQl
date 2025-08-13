using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class CreateServiceDto
    {
        public string? ImageUrl { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public Guid? CategoryId { get; set; }
        
    }
}
