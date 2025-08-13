using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "updatedat";
        public string SortDirection { get; set; } = "desc";

        public Guid? CategoryId { get; set; }
        public string? ComplaintStatus { get; set; }
        public string? DateFilter { get; set; }
        public string SearchTerm { get; set; } = "";
        public Guid? IssueTypeId { get; set; }
    }
}
