using SawirahMunicipalityWeb.Enums;
using System.ComponentModel.DataAnnotations;

namespace SawirahMunicipalityWeb.Models
{
    public class CreateComplaintDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

      
        public Guid IssueId { get; set; }

        public string Description { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsSeen { get; set; } = false;
    }
}
