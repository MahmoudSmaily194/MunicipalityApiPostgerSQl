using System.ComponentModel.DataAnnotations;

namespace SawirahMunicipalityWeb.Models
{
    public class CreateComplaintFormDataDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

     
        public string PhoneNumber { get; set; }

        public Guid IssueId { get; set; }

        public string Description { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // ✅ Image File (uploaded by user)
        public IFormFile? Image { get; set; }
    }
}
