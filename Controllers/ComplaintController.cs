using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.ComplaintServices;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController(IComplaintService complaintService) : ControllerBase
    {
        [HttpPost("create_complaint")]
        public async Task<IActionResult> CreateComplaint([FromForm] CreateComplaintFormDataDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // 1. حفظ الصورة في السيرفر
                string? imageUrl = null;

                if (request.Image is { Length: > 0 })
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "complaints");
                    Directory.CreateDirectory(folderPath); // تأكد من وجود المجلد

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await request.Image.CopyToAsync(stream);

                    // إنشاء رابط للوصول إلى الصورة
                    imageUrl = $"/images/complaints/{fileName}";
                }
                var createDto = new CreateComplaintDto
                {
                    ImageUrl = imageUrl,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    IssueId = request.IssueId,
                    Description = request.Description,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                };
                var complaint = await complaintService.CreateComplaintAsync(createDto);
                return Created(string.Empty, new
                {
                    message = "Complaint created successfully",
                    complaintId = complaint.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }

        [HttpPost("create_complaint_issueType")]

        public async Task<IActionResult> CreateComplaintIssueType(CreateIssueTypeDto request)
        {
            var res = await complaintService.CreateComplaintIssueAsync(request);

            if (res is null)
            {
                return Conflict(new { message = "Issue type already exists" });
            }
            return Created(string.Empty, new
            {
                message = "Complaint created successfully",
                res
            });
        }

        [HttpGet("get_complaints")]

        public async Task<IActionResult> getComplaintPaged([FromQuery] PaginationParams paginationParams)
        {
            var res = await complaintService.getComplaintPagedAsync(paginationParams);
            return Ok(res);
        }


        [HttpGet("get_complaint_by_id")]
        public async Task<IActionResult> getComplaint([FromQuery] Guid Id)
        {
            var res = await complaintService.getComplaint(Id);
            if (res is null) { return NotFound("complaint not found"); }
            return Ok(res);
        }

        [HttpGet("get_public_complaints")]
        public async Task<IActionResult> GetPublicComplaints([FromQuery] PaginationParams paginationParams)
        {
            var res = await complaintService.getPublicComplaintPagedAsync(paginationParams);
            return Ok(res);
        }


        [HttpGet("issue_types")]
        public async Task<IActionResult> getComplaintIssues()
        {
            var res = await complaintService.getComplaintIssueAsync();

            return Ok(res);
        }
        [HttpPut("update_complaint")]
        public async Task<IActionResult> UpdateComplaint([FromQuery] Guid Id, UpdateComplaintDto updateComplaintDto)
        {
            var res = await complaintService.updateComplaint(Id, updateComplaintDto);
            if (res is null) { return NotFound("complaint not found"); }
            return Ok(res);
        }

        [HttpDelete("delete_complaint_issueType")]
        public async Task<IActionResult> DeleteComplaintIssueType([FromQuery] Guid Id)
        {
            var res = await complaintService.DeleteIssueTypeAsync(Id);
            if (res == false)
            {
                return NotFound("the issue type was not found");
            }
            return Ok(res);
        }

        [HttpPut("set_complaint_seen")]

        public async Task<IActionResult> SetComplaintAsSeen([FromQuery] Guid Id)
        {
            var res = await complaintService.SetComplaintAsSeen(Id);
            if (res == false) { return NotFound("complaint is not found"); }
            return Ok(res);
        }

        [HttpGet("get-seen-complaints")]
        public async Task<IActionResult> GetUnseenComplaints([FromQuery] PaginationParams paginationParams)
        {
            var res =await complaintService.GetUnseenComplaints(paginationParams);
            return Ok(res);
        }
    }
}
