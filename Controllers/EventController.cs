using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.EventsServices;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(IEventsService eventsService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("create_event")]

        public async Task<IActionResult> CreateEvent([FromForm] CreateEventFormDataDto request)
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
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events");
                    Directory.CreateDirectory(folderPath); // تأكد من وجود المجلد

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await request.Image.CopyToAsync(stream);

                    // إنشاء رابط للوصول إلى الصورة
                    imageUrl = $"/images/events/{fileName}";
                }
                var createDto = new CreateEventDto
                {
                    Title = request.Title,
                    Description = request.Description,
                   Location=request.Location,
                   Date=request.Date,
                    ImageUrl = imageUrl
                };
                var events = await eventsService.CreateEventAsync(createDto);
                return CreatedAtAction(nameof(GetEventBySlug), new { slug = events.Slug }, events);
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


        [HttpGet("get_event")]
        public async Task<IActionResult> GetEventBySlug([FromQuery] string slug)
        {
            var res = await eventsService.GetBySlugAsync(slug);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [HttpGet("get_all_events")]
        public async Task<IActionResult> GetAllEvents([FromQuery]  PaginationParams paginationParams)
        {
            var res = await eventsService.GetAllEvents(paginationParams);
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_event")]

        public async Task<IActionResult> UpdateEvent([FromQuery] Guid id, [FromBody] CreateEventDto request)
        {
            var res = await eventsService.UpdateEventAsync(id, request);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_event")]

        public async Task<IActionResult> DeleteEvent([FromQuery] Guid id)
        {
            var res = await eventsService.DeleteEventAsync(id);
            if (res == false)
            {
                return NotFound();
            }
            return Ok("deleted");
        }
    }
}
