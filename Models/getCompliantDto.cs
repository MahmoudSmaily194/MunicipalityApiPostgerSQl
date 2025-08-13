using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class getCompliantDto
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public Guid IssueId { get; set; }
        public string IssueName { get; set; }
        public string Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Visibility { get; set; }
        public string Status { get; set; }
        public ComplaintImportance? Importance { get; set; }
    }
}
