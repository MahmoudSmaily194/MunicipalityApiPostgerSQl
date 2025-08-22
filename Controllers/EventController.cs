using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.EventsServices;
using SawirahMunicipalityWeb.Services.ImageService;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        public readonly IEventsService _eventsService;
        private readonly SupabaseImageService _imageService;
        public EventController(IEventsService eventsService , SupabaseImageService imageService)
        {
            _eventsService = eventsService;
            _imageService = imageService;
        }

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
                string? imageUrl = null;
                if (request.Image is { Length: > 0 })
                {
                    imageUrl = await _imageService.UploadImageAsync(request.Image, "sawirah-images");
                }
                var createDto = new CreateEventDto
                {
                    Title = request.Title,
                    Description = request.Description,
                   Location=request.Location,
                   Date=request.Date,
                    ImageUrl = imageUrl
                };
                var events = await _eventsService.CreateEventAsync(createDto);
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
            var res = await _eventsService.GetBySlugAsync(slug);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [HttpGet("get_all_events")]
        public async Task<IActionResult> GetAllEvents([FromQuery]  PaginationParams paginationParams)
        {
            var res = await _eventsService.GetAllEvents(paginationParams);
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_event")]

        public async Task<IActionResult> UpdateEvent([FromQuery] Guid id, [FromBody] CreateEventDto request)
        {
            var res = await _eventsService.UpdateEventAsync(id, request);
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
            var res = await _eventsService.DeleteEventAsync(id);
            if (res == false)
            {
                return NotFound();
            }
            return Ok("deleted");
        }
    }
}
