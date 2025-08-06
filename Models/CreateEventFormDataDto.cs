namespace SawirahMunicipalityWeb.Models
{
    public class CreateEventFormDataDto
    {
        public IFormFile? Image { get; set; }
        public string? Location { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; } = string.Empty;
    }
}
