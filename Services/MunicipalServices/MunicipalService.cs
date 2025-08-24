using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Helpers;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.MunicipalServices
{
    public class MunicipalService : IMunicipalService
    {
        private readonly DBContext _context;
        public MunicipalService(DBContext context)
        {
            _context = context;
        }


        public async Task<ServicesCategories?> CreateServiceCategoryAsync(CreateServiceCategoryDto dto)
        {
            // Find if a category with the same name exists (even deleted ones)
            var existingCategory = await _context.ServicesCategories
                .FirstOrDefaultAsync(e => e.Name == dto.Name);

            if (existingCategory != null)
            {
                if (existingCategory.IsDeleted)
                {
                    // Reactivate soft-deleted category
                    existingCategory.IsDeleted = false;
                    _context.ServicesCategories.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    return existingCategory;
                }

                // Already exists and not deleted → reject
                return null;
            }

            // Otherwise, create a new category
            var newServiceCategory = new ServicesCategories
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                IsDeleted = false
            };

            _context.ServicesCategories.Add(newServiceCategory);
            await _context.SaveChangesAsync();
            return newServiceCategory;
        }


        public async Task<Service?> CreateService(CreateServiceDto dto)
        {
            var slug = await SlugHelper.GenerateUniqueSlug<Service>(
            dto.Title,
            _context,
            s => s.Slug
            );
            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Slug = slug,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Status = dto.Status,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<PaginatedResponse<GetServiceDto>> GetServicesAsync(PaginationParams paginationParams)
        {
            var query = _context.Services
                .Include(s => s.Category) // include Category so CategoryName is available
                .OrderByDescending(c => c.CreatedAt)
                .AsQueryable();

            if (paginationParams.CategoryId.HasValue)
            {
                query = query.Where(n => n.CategoryId == paginationParams.CategoryId);
            }

            if (!string.IsNullOrEmpty(paginationParams.SearchTerm))
            {
                var term = paginationParams.SearchTerm.ToLower();
                query = query.Where(n => n.Title.ToLower().Contains(term) || n.Description.ToLower().Contains(term));
            }

            var sortBy = paginationParams.SortBy?.ToLower();
            var sortDirection = paginationParams.SortDirection?.ToLower();

            if (sortBy == "updatedat")
            {
                query = sortDirection == "asc"
                    ? query.OrderBy(n => n.UpdatedAt)
                    : query.OrderByDescending(n => n.UpdatedAt);
            }
            else if (sortBy == "title")
            {
                query = sortDirection == "asc"
                    ? query.OrderBy(n => n.Title)
                    : query.OrderByDescending(n => n.Title);
            }

            // Project into DTO
            var projectedQuery = query.Select(s => new GetServiceDto
            {
                Id = s.Id,
                ImageUrl = s.ImageUrl,
                Title = s.Title,
                Description = s.Description,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                Slug = s.Slug,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null
            });

            // Paginate after projection
            var paginated = await projectedQuery.ToPaginatedListAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            return paginated;
        }

        public async Task<List<ServicesCategories>> GetServiceCategoriesAsync()
        {
            var categories = await _context.ServicesCategories.OrderByDescending(x => x.Id).ToListAsync();
            return categories;
        }

        public async Task<Service?> UpdateServiceAsync(Guid id, CreateServiceDto dto)
        {
            var updatedService = await _context.Services.FindAsync(id);
            if (updatedService is null) return null;

            updatedService.Title = dto.Title;
            updatedService.Description = dto.Description;
            updatedService.Status = dto.Status;
            updatedService.ImageUrl = dto.ImageUrl;
            updatedService.Slug = await SlugHelper.GenerateUniqueSlug<Service>(
            dto.Title,
            _context,
            s => s.Slug
            );

            await _context.SaveChangesAsync();
            return updatedService;
        }

        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service is null) return false;
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServicesCategories> UpdateServiceCategoryAsync(Guid id, CreateServiceCategoryDto dto)
        {
            var updatedCateg = await _context.ServicesCategories.FindAsync(id);
            if (updatedCateg is null)
            {
                return null;
            }
            updatedCateg.Name = dto.Name;
            await _context.SaveChangesAsync();
            return updatedCateg;
        }

        public async Task<bool> DeleteServiceCategoryAsync(Guid id)
        {
            var category = await _context.ServicesCategories.FindAsync(id);
            if (category is null) return false;
            category.IsDeleted = true;
            _context.ServicesCategories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GetServiceDto?> GetServiceByIdAsync(Guid id)
        {
            var service = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service is null) return null;

            return new GetServiceDto
            {
                Id = service.Id,
                ImageUrl = service.ImageUrl,
                Title = service.Title,
                Description = service.Description,
                Status = service.Status,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt,
                Slug = service.Slug,
                CategoryId = service.CategoryId,
                CategoryName = service.Category != null ? service.Category.Name : null
            };
        }
    }

}



