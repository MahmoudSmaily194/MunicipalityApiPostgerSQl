using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Helpers;
using SawirahMunicipalityWeb.Models;
using System.Globalization;

namespace SawirahMunicipalityWeb.Services.EventsServices
{
    public class EventsService : IEventsService
    {
        private readonly DBContext _context;
        public EventsService(DBContext context)
        {
            _context = context;
        }

        public async Task<Event?> CreateEventAsync(CreateEventDto dto)
        {
            var slug = await SlugHelper.GenerateUniqueSlug<Event>(
                        dto.Title,
                        _context,
                        e => e.Slug
);

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                ImageUrl = dto.ImageUrl,
                Location = dto.Location,
                Slug = slug
            };
            _context.Events.AddAsync(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }



        public async Task<Event?> GetBySlugAsync(string slug)
        {
            return await _context.Events
                .FirstOrDefaultAsync(e => e.Slug == slug);
        }
        public async Task<PaginatedResponse<Event>> GetAllEvents(PaginationParams paginationParams)
        {
            var query = _context.Events.AsQueryable();
            //var sortBy = paginationParams.SortBy?.ToLower();
            //var sortDirection = paginationParams.SortDirection?.ToLower();
            //if (sortBy == "updatedat")
            //{
            //    query = sortDirection == "asc"
            //        ? query.OrderBy(n => n)
            //        : query.OrderByDescending(n => n.UpdatedAt);
            //}
            //else if (sortBy == "title")
            //{
            //    query = sortDirection == "asc"
            //        ? query.OrderBy(n => n.Title)
            //        : query.OrderByDescending(n => n.Title);
            //}
            return await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<Event?> UpdateEventAsync(Guid id, CreateEventDto dto)
        {
            var updatedEvent = await _context.Events.FindAsync(id);
            if (updatedEvent is null)
            {
                return null;
            }
            updatedEvent.Title = dto.Title;
            updatedEvent.Description = dto.Description;
            updatedEvent.Date = dto.Date;
            updatedEvent.ImageUrl = dto.ImageUrl;
            updatedEvent.Location = dto.Location;
            updatedEvent.Slug = await SlugHelper.GenerateUniqueSlug<Event>(
    dto.Title,
    _context,
    e => e.Slug
);
            await _context.SaveChangesAsync();
            return updatedEvent;
        }

        public async Task<bool> DeleteEventAsync(Guid id)
        {
            var res = await _context.Events.FindAsync(id);
            if (res is null) { return false; }
            _context.Events.Remove(res);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
