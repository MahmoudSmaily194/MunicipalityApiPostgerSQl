using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.ComplaintServices;
using SawirahMunicipalityWeb.Services.ImageService;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        public readonly IComplaintService _complaintService;
        private readonly SupabaseImageService _imageService;
        public ComplaintController(IComplaintService complaintService, SupabaseImageService imageService) 
        {
            _complaintService= complaintService;
            _imageService = imageService;
        }

        [HttpPost("create_complaint")]
        public async Task<IActionResult> CreateComplaint([FromForm] CreateComplaintFormDataDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                string? imageUrl = null;
                if (request.Image is { Length: > 0 })
                {
                    imageUrl = await _imageService.UploadImageAsync(request.Image, "sawirah-images");
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
                var complaint = await _complaintService.CreateComplaintAsync(createDto);
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
        [Authorize(Roles = "Admin")]
        [HttpPost("create_complaint_issueType")]

        public async Task<IActionResult> CreateComplaintIssueType(CreateIssueTypeDto request)
        {
            var res = await _complaintService.CreateComplaintIssueAsync(request);

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
        [Authorize(Roles = "Admin")]
        [HttpGet("get_complaints")]

        public async Task<IActionResult> getComplaintPaged([FromQuery] PaginationParams paginationParams)
        {
            var res = await _complaintService.getComplaintPagedAsync(paginationParams);
            return Ok(res);
        }


        [HttpGet("get_complaint_by_id")]
        public async Task<IActionResult> getComplaint([FromQuery] Guid Id)
        {
            var res = await _complaintService.getComplaint(Id);
            if (res is null) { return NotFound("complaint not found"); }
            return Ok(res);
        }

        [HttpGet("get_public_complaints")]
        public async Task<IActionResult> GetPublicComplaints([FromQuery] PaginationParams paginationParams)
        {
            var res = await _complaintService.getPublicComplaintPagedAsync(paginationParams);
            return Ok(res);
        }


        [HttpGet("issue_types")]
        public async Task<IActionResult> getComplaintIssues()
        {
            var res = await _complaintService.getComplaintIssueAsync();

            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_complaint")]
        public async Task<IActionResult> UpdateComplaint([FromQuery] Guid Id, UpdateComplaintDto updateComplaintDto)
        {
            var res = await _complaintService.updateComplaint(Id, updateComplaintDto);
            if (res is null) { return NotFound("complaint not found"); }
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_complaint_issueType")]
        public async Task<IActionResult> DeleteComplaintIssueType([FromQuery] Guid Id)
        {
            var res = await _complaintService.DeleteIssueTypeAsync(Id);
            if (res == false)
            {
                return NotFound("the issue type was not found");
            }
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("set_complaint_seen")]

        public async Task<IActionResult> SetComplaintAsSeen([FromQuery] Guid Id)
        {
            var res = await _complaintService.SetComplaintAsSeen(Id);
            if (res == false) { return NotFound("complaint is not found"); }
            return Ok(res);
        }

        [HttpGet("get-seen-complaints")]
        public async Task<IActionResult> GetUnseenComplaints([FromQuery] PaginationParams paginationParams)
        {
            var res =await _complaintService.GetUnseenComplaints(paginationParams);
            return Ok(res);
        }
    }
}
