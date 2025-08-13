using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Enums;
using SawirahMunicipalityWeb.Helpers;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.ComplaintServices
{
    public class ComplaintService : IComplaintService
    {
        private readonly DBContext _context;

        public ComplaintService(DBContext context)
        {
            _context = context;
        }

        public async Task<Complaint?> CreateComplaintAsync(CreateComplaintDto dto)
        {
            var issue = await _context.ComplaintIssues.FindAsync(dto.IssueId);

            if (issue == null)
            {
                throw new InvalidOperationException($"Invalid issue ID: {dto.IssueId}");
            }
            var complaint = new Complaint
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                IssueId = dto.IssueId,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                Status = ComplaintStatus.Pending,
                Visibility = Visibility.Private,
            };

            try
            {
                Console.WriteLine("📝 Attempting to save complaint...");
                await _context.Complaints.AddAsync(complaint);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Complaint saved successfully");
                return complaint;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed to save complaint:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("🔍 Inner Exception:");
                    Console.WriteLine(ex.InnerException.Message);
                }

                throw; // نعيد الاستثناء حتى يظهر في الـ API response
            }
        }

        public async Task<ComplaintIssue?> CreateComplaintIssueAsync(CreateIssueTypeDto dto)
        {
            bool issueExist = await _context.ComplaintIssues.AnyAsync(e => e.IssueName == dto.IssueName);
            if (issueExist)
            {
                return null;
            }
            var issue = new ComplaintIssue
            {
                Id = Guid.NewGuid(),
                IssueName = dto.IssueName,
            };
            await _context.AddAsync(issue);
            await _context.SaveChangesAsync();
            return issue;
        }


        public async Task<PaginatedResponse<getCompliantDto>> getComplaintPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var query = _context.Complaints
                    .Include(c => c.ComplaintIssue).OrderByDescending(c => c.CreatedAt)
                    .Select(c => new getCompliantDto
                    {
                        Id = c.Id,
                        ImageUrl = c.ImageUrl,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        IssueId = c.IssueId,
                        IssueName = c.ComplaintIssue.IssueName,
                        Description = c.Description,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        Visibility = c.Visibility.ToString(),
                        Status = c.Status.ToString(),
                        Importance = c.Importance,

                    });

                return await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            }
            catch (Exception ex)
            {
                // Log ex.Message and ex.StackTrace appropriately
                throw;
            }
        }


        public async Task<List<ComplaintIssue>> getComplaintIssueAsync()
        {
            var res = await _context.ComplaintIssues.OrderBy(e => e.Id).ToListAsync();
            return res;
        }

        public async Task<bool> DeleteIssueTypeAsync(Guid id)
        {
            var res = await _context.ComplaintIssues.FindAsync(id);
            if (res == null)
            {
                return false;
            }
            _context.ComplaintIssues.Remove(res);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<getCompliantDto?> getComplaint(Guid Id)
        {
            var res = await _context.Complaints
                .Include(c => c.ComplaintIssue)
                .Where(c => c.Id == Id)
                .Select(c => new getCompliantDto
                {
                    Id = c.Id,
                    ImageUrl = c.ImageUrl,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    IssueId = c.IssueId,
                    IssueName = c.ComplaintIssue.IssueName,
                    Description = c.Description,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Visibility = c.Visibility.ToString(),
                    Status = c.Status.ToString(),
                    Importance = c.Importance,
                })
                .FirstOrDefaultAsync();

            return res;
        }

        public async Task<Complaint?> updateComplaint(Guid Id, UpdateComplaintDto dto)
        {
            var exist = await _context.Complaints.FindAsync(Id);
            if (exist == null)
            {
                return null;
            }
            exist.Status = dto.Status;
            exist.Visibility = dto.Visibility;
            exist.Importance = dto.Importance;
            await _context.SaveChangesAsync();
            return exist;
        }

        public async Task<PaginatedResponse<getCompliantDto>> getPublicComplaintPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var query = _context.Complaints
                     .Include(c => c.ComplaintIssue).OrderByDescending(c => c.CreatedAt)
                     .Where(c => c.Visibility == Visibility.Public)
                     .Select(c => new getCompliantDto
                     {
                         Id = c.Id,
                         ImageUrl = c.ImageUrl,
                         FullName = c.FullName,
                         PhoneNumber = c.PhoneNumber,
                         IssueId = c.IssueId,
                         IssueName = c.ComplaintIssue.IssueName,
                         Description = c.Description,
                         Latitude = c.Latitude,
                         Longitude = c.Longitude,
                         CreatedAt = c.CreatedAt,
                         UpdatedAt = c.UpdatedAt,
                         Visibility = c.Visibility.ToString(),
                         Status = c.Status.ToString(),
                         Importance = c.Importance
                     });
                if (paginationParams.IssueTypeId != null && paginationParams.IssueTypeId != Guid.Empty)
                {
                    query = query.Where(c => c.IssueId == paginationParams.IssueTypeId);
                }
                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    query = query.Where(c => c.Description.Contains(paginationParams.SearchTerm));
                }
                if (!string.IsNullOrWhiteSpace(paginationParams.ComplaintStatus))
                {
                    if (Enum.TryParse<ComplaintStatus>(paginationParams.ComplaintStatus, true, out var complaintStatus))
                    {
                        query = query.Where(c => c.Status == paginationParams.ComplaintStatus);
                    }

                }
                return await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            }
            catch (Exception ex)
            {
                // Log ex.Message and ex.StackTrace appropriately
                throw;
            }
        }

        public async Task<PaginatedResponse<getCompliantDto>> GetUnseenComplaints(PaginationParams paginationParams)
        {
            try
            {
                var query = _context.Complaints
                                     .Include(c => c.ComplaintIssue).OrderByDescending(c => c.CreatedAt)
                                     .Where(c => c.IsSeen == false)
                                     .Select(c => new getCompliantDto
                                     {
                                         Id = c.Id,
                                         ImageUrl = c.ImageUrl,
                                         FullName = c.FullName,
                                         PhoneNumber = c.PhoneNumber,
                                         IssueId = c.IssueId,
                                         IssueName = c.ComplaintIssue.IssueName,
                                         Description = c.Description,
                                         Latitude = c.Latitude,
                                         Longitude = c.Longitude,
                                         CreatedAt = c.CreatedAt,
                                         UpdatedAt = c.UpdatedAt,
                                         Visibility = c.Visibility.ToString(),
                                         Status = c.Status.ToString(),
                                     });
                return await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> SetComplaintAsSeen(Guid Id)
        {
            var res = await _context.Complaints.FindAsync(Id);
            if (res == null) return false;
            res.IsSeen = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
