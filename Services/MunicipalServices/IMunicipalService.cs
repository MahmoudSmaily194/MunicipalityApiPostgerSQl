using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.MunicipalServices
{
    public interface IMunicipalService
    {
        Task<ServicesCategories> CreateServiceCategoryAsync(CreateServiceCategoryDto dto);
        Task<Service?> CreateService(CreateServiceDto dto);
        Task<GetServiceDto> GetServiceByIdAsync(Guid id);
        Task<PaginatedResponse<GetServiceDto>> GetServicesAsync(PaginationParams paginationParams);
        Task<List<ServicesCategories>> GetServiceCategoriesAsync();
        Task<Service?> UpdateServiceAsync(Guid id, CreateServiceDto dto);
        Task<bool> DeleteServiceAsync(Guid id);
        Task<ServicesCategories> UpdateServiceCategoryAsync(Guid id ,CreateServiceCategoryDto dto);
        Task<bool> DeleteServiceCategoryAsync(Guid id);
    }
}
