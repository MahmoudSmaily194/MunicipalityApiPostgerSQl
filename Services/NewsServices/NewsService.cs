
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Enums;
using SawirahMunicipalityWeb.Helpers;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.NewsServices
{
    public class NewsService : INewsService
    {

        private readonly DBContext _context;

        public NewsService(DBContext context)
        {
            _context = context;
        }

        public async Task<News?> CreateNewsItemAsync(CreateNewsItemDto request)
        {
            var slug = await SlugHelper.GenerateUniqueSlug<News>(
    request.Title,
    _context,
    n => n.Slug
);
            var news = new News
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Visibility = request.Visibility,
                ImageUrl = request.ImageUrl,
                Slug = slug,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
            return news;

        }


        public async Task<News?> GetBySlugAsync(string slug)
        {
            var newsItem = await _context.News
     .FirstOrDefaultAsync(n => EF.Functions.ILike(n.Slug, slug)
                               && n.Visibility == Visibility.Public);
            if (newsItem is null)
            {
                return null;
            }
            return new News
            {
                Title = newsItem.Title,
                Description = newsItem.Description,
                UpdatedAt = newsItem.UpdatedAt,
                ImageUrl = newsItem.ImageUrl,
                Id = newsItem.Id,
                Slug = newsItem.Slug,
                Visibility = newsItem.Visibility,

            };

        }

        public async Task<PaginatedResponse<News>> GetVisibleAsync(PaginationParams paginationParams)
        {
            var query = _context.News.OrderByDescending(c => c.CreatedAt).AsQueryable();
            if (!string.IsNullOrEmpty(paginationParams.DateFilter))
            {
                DateTime now = DateTime.UtcNow;

                switch (paginationParams.DateFilter.ToLower())
                {
                    case "today":
                        var todayStart = now.Date;
                        var todayEnd = todayStart.AddDays(1);
                        query = query.Where(n => n.UpdatedAt >= todayStart && n.UpdatedAt < todayEnd);
                        break;

                    case "lastweek":
                        var lastWeekStart = now.AddDays(-7);
                        query = query.Where(n => n.UpdatedAt >= lastWeekStart && n.UpdatedAt <= now);
                        break;

                    case "lastmonth":
                        var lastMonthStart = now.AddMonths(-1);
                        query = query.Where(n => n.UpdatedAt >= lastMonthStart && n.UpdatedAt <= now);
                        break;
                    case "lastyear":
                        var lastYearStart = now.AddYears(-1);
                        query = query.Where(n => n.UpdatedAt >= lastYearStart && n.UpdatedAt <= now);
                        break;
                }
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

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                query = query.Where(p => p.Title.ToLower().Contains(paginationParams.SearchTerm.ToLower()) || p.Description.Contains(paginationParams.SearchTerm.ToLower()));
            }


            var paginated = await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return paginated;
        }
        public async Task<PaginatedResponse<News>> GetAllAsync(PaginationParams paginationParams)
        {

            var query = _context.News.OrderByDescending(c => c.CreatedAt).AsQueryable();
            return await query.ToPaginatedListAsync(1, 10);
        }

        public async Task<News?> UpdateNewsItemAsync(Guid id, UpdateNewsItemDto dto)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem is null) return null;
            newsItem.Title = dto.Title;
            newsItem.Description = dto.Description;
            newsItem.Visibility = dto.Visibility;
            newsItem.ImageUrl = dto.ImageUrl;

            newsItem.Slug = await SlugHelper.GenerateUniqueSlug<News>(
dto.Title,
_context,
n => n.Slug
);

            newsItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return newsItem;
        }

        public async Task<bool> DeleteNewsItemAsync(Guid id)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem is null) return false;
            _context.News.Remove(newsItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
