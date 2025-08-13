using SawirahMunicipalityWeb.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SawirahMunicipalityWeb.Entities
{
    public class Complaint
    {
        public Guid Id { get; set; }

        public string? ImageUrl { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public Guid IssueId { get; set; }
        public string IssueName => ComplaintIssue?.IssueName ?? string.Empty;
        public string Description { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public Visibility Visibility { get; set; }=Visibility.Private;

        [ForeignKey("IssueId")]
        public ComplaintIssue ComplaintIssue { get; set; } = null!;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public ComplaintImportance Importance { get; set; } = ComplaintImportance.NotImportant;
        public bool IsSeen { get; set; } = false;


    }
}
