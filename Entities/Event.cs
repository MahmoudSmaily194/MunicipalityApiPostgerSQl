using System.ComponentModel.DataAnnotations.Schema;

namespace SawirahMunicipalityWeb.Entities
{
    public class Event
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }=string.Empty;
        public DateTime? Date { get; set; }
        public string Title { get; set; }=string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool MarkedAsDone { get; set; } = false;
        public string Slug { get; set; }

        [NotMapped]
        public bool IsDone => MarkedAsDone || (Date.HasValue && DateTime.UtcNow > Date.Value);
    }
}
