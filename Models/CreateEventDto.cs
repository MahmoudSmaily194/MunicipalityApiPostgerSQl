namespace SawirahMunicipalityWeb.Models
{
    public class CreateEventDto
    {
        public string? ImageUrl { get; set; }
        public string? Location { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string Title { get; set; } 
        public string? Description { get; set; } = string.Empty;
    }
}
