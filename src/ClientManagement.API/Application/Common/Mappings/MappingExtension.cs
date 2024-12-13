using ClientManagementAPI.Application.Common.Models;

namespace ClientManagementAPI.Application.Common.Mappings;

public static class MappingExtension
{
    public static async Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await PaginatedList<TDestination>.CreateAsync(queryable, pageNumber, pageSize, cancellationToken);
    }
}