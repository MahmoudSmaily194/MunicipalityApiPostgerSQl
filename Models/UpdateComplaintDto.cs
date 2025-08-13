using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class UpdateComplaintDto
    {
        public Visibility Visibility { get; set; } = Visibility.Private;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public ComplaintImportance Importance { get; set; } 
    }
}
