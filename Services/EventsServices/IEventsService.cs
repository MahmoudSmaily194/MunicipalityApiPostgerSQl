using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.EventsServices
{
    public interface IEventsService
    {
        Task<Event?> CreateEventAsync(CreateEventDto dto);
        Task<Event?> GetBySlugAsync(string slug);
        Task<PaginatedResponse<Event>> GetAllEvents(PaginationParams paginationParams);
        Task<Event?> UpdateEventAsync(Guid id ,CreateEventDto dto);
        Task<bool> DeleteEventAsync(Guid id);
    }
}
