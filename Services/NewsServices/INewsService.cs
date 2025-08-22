using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.NewsServices
{
    public interface INewsService
    {
        Task<News?> CreateNewsItemAsync(CreateNewsItemDto request);
        Task<News?> GetBySlugAsync(string slug);
        Task<News?> GetAllNewsItemBySlugAsync(string slug);
        Task<PaginatedResponse<News>> GetVisibleAsync(PaginationParams paginationParams);
        Task<PaginatedResponse<News>> GetAllAsync(PaginationParams paginationParams);
        Task<News?> UpdateNewsItemAsync(Guid id , UpdateNewsItemDto dto);
        Task<bool> DeleteNewsItemAsync(Guid id);
    }
}
