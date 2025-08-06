using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PaginatedResponse<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize)
        {
            // Count total items in the query
            var count = await source.CountAsync();

            // Apply Skip and Take to get items for the requested page
            var items = await source.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Build and return the PaginatedResponse
            return new PaginatedResponse<T>
            {
                Items = items,
                TotalCount = count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }
}
