using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.ImageService;
using SawirahMunicipalityWeb.Services.NewsServices;
using static System.Net.Mime.MediaTypeNames;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly SupabaseImageService _imageService;

        public NewsController(INewsService newsService, SupabaseImageService imageService)
        {
            _newsService = newsService;
            _imageService = imageService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("create_news")]
        public async Task<IActionResult> CreateNews([FromForm] CreateNewsItemFormData request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                string? imageUrl = null;
                if (request.Image is { Length: > 0 })
                {
                    imageUrl = await _imageService.UploadImageAsync(request.Image, "sawirah-images");
                }

                var createDto = new CreateNewsItemDto
                {
                    Title = request.Title,
                    Description = request.Description,
                    Visibility = request.Visibility,
                    ImageUrl = imageUrl,
                };

                var news = await _newsService.CreateNewsItemAsync(createDto);

                return CreatedAtAction(nameof(GetNewsBySlug), new { slug = news.Slug }, news);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating news.", detail = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsBySlug([FromQuery] string slug)
        {
            var news = await _newsService.GetBySlugAsync(slug);
            if (news == null)
                return NotFound();

            return Ok(news);
        }


        [HttpGet("get_visible_news")]
        public async Task<IActionResult> GetVisibleNews([FromQuery] PaginationParams paginationParams)
        {
            var result = await _newsService.GetVisibleAsync(paginationParams);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("get_all_news")]
        public async Task<IActionResult> GetAllNews([FromQuery] PaginationParams paginationParams)
        {
            var result = await _newsService.GetAllAsync(paginationParams);

            if (result.Items == null || result.Items.Count == 0)
            {
                return NotFound(new { message = "No news found for the given filters." });
            }

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_newsItem")]
        public async Task<IActionResult> UpdateNews([FromQuery] Guid id, [FromBody] UpdateNewsItemDto dto)
        {
            var updated = await _newsService.UpdateNewsItemAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "News item not found." });

            return Ok(updated);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_newsItem")]

        public async Task<IActionResult> DeleteNewsItem([FromQuery] Guid id)
        {
            var deletedItem = await _newsService.DeleteNewsItemAsync(id);

            if (deletedItem == false)
            {
                return NotFound(new { message = "News item not found." });
            }
            return NoContent();
        }
    }
}
