using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.ImageService;
using SawirahMunicipalityWeb.Services.MunicipalServices;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MunicipalServiceController : ControllerBase
    {
        private readonly IMunicipalService _municipalService;
        private readonly SupabaseImageService _imageService;

        public MunicipalServiceController(IMunicipalService municipalService, SupabaseImageService imageService)
        {
            _municipalService = municipalService;
            _imageService = imageService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create_service_category")]
        public async Task<IActionResult> CreateServiceCategory(CreateServiceCategoryDto request)
        {
            var res = await _municipalService.CreateServiceCategoryAsync(request);
            if (res is null)
            {
                return Conflict(new { message = "Category name already exists." });
            }
            return StatusCode(201, res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("create_service")]
        public async Task<IActionResult> CreateService([FromForm] CreateFormDataService request)
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
                var createDto = new CreateServiceDto
                {
                    Title = request.Title,
                    Description = request.Description,
                    Status = request.Status,
                    ImageUrl = imageUrl,
                    CategoryId = request.CategoryId
                };


                var res = await _municipalService.CreateService(createDto);
                return StatusCode(201, res);
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

        [HttpGet("service_categories")]
        public async Task<IActionResult> GetAllSeviceCategories()
        {
            var res = await _municipalService.GetServiceCategoriesAsync();
            if (res is null)
            {
                return NotFound("there is no service categories ");
            }
            return Ok(res);
        }
        [AllowAnonymous]
        [HttpGet("services")]
        public async Task<IActionResult> GetAllSevices([FromQuery] PaginationParams paginationParams)
        {
            var res = await _municipalService.GetServicesAsync(paginationParams);
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_service")]
        public async Task<IActionResult> UpdateService([FromQuery] Guid id, CreateServiceDto request)
        {
            var res = await _municipalService.UpdateServiceAsync(id, request);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_service")]
        public async Task<IActionResult> DeleteService([FromQuery] Guid id)
        {
            var res = await _municipalService.DeleteServiceAsync(id);
            if (res == false)
            {
                return NotFound("Service not found");
            }
            return Ok("deleted");
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_service_category")]
        public async Task<IActionResult> UpdateServiceCategory([FromQuery] Guid categoryId, CreateServiceCategoryDto request)
        {
            var res = await _municipalService.UpdateServiceCategoryAsync(categoryId, request);
            if (res == null)
            {
                return NotFound("category not found");
            }
            return Ok(res);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_service_category")]
        public async Task<IActionResult> DeleteServiceCategory([FromQuery] Guid id)
        {
            var res = await _municipalService.DeleteServiceCategoryAsync(id);
            if (res == false)
            {
                return NotFound("Category not found");
            }
            return Ok("deleted");
        }

        [HttpGet("get_service_byId")]
        public async Task<IActionResult> GetServiceById([FromQuery] Guid id)
        {
            var res = await _municipalService.GetServiceByIdAsync(id);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
    }
}
