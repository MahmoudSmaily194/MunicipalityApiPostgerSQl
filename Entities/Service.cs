using SawirahMunicipalityWeb.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SawirahMunicipalityWeb.Entities
{
    public class Service
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Slug { get; set; }
        [NotMapped]
        public string? CategoryName => Category?.Name;
        
        // Fk
        public Guid? CategoryId { get; set; }

        // Link to the Category Entity
        public ServicesCategories Category { get; set; } = null!;
    }
}
