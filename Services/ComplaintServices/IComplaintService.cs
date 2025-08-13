using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.ComplaintServices
{
    public interface IComplaintService
    {
        Task<Complaint?> CreateComplaintAsync(CreateComplaintDto dto);
        Task<ComplaintIssue?> CreateComplaintIssueAsync(CreateIssueTypeDto dto);
        Task<List<ComplaintIssue>> getComplaintIssueAsync();
        Task<PaginatedResponse<getCompliantDto>> getComplaintPagedAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<getCompliantDto>> getPublicComplaintPagedAsync(PaginationParams paginationParams);
        Task<bool> DeleteIssueTypeAsync(Guid id);
        Task<getCompliantDto?> getComplaint(Guid Id);
        Task<Complaint?> updateComplaint(Guid Id , UpdateComplaintDto dto);
        Task<PaginatedResponse<getCompliantDto>> GetUnseenComplaints( PaginationParams paginationParams);
        Task<bool> SetComplaintAsSeen(Guid Id);
    }
}
