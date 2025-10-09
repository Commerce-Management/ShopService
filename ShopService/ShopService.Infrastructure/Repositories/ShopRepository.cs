using Microsoft.EntityFrameworkCore;
using ShopService.Core.Entities;
using ShopService.Infrastructure.Context;
using ShopService.Infrastructure.Interfaces.Entities;
using ShopService.Infrastructure.Repositories.Base;

namespace ShopService.Infrastructure.Repositories;

public class ShopRepository(ShopDbContext context) : Repository<Shop>(context), IShopRepository
{
    private IQueryable<Shop> GetShopQuery() =>
        Entities.AsNoTracking();

    public async Task<IEnumerable<Shop>> GetAllShopsAsync() =>
        await GetShopQuery()
            .ToListAsync();

    public async Task<Shop?> GetShopByIdAsync(Guid id) =>
        await GetShopQuery()
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Shop?> GetShopBySubdomainAsync(string subdomain) =>
        await GetShopQuery()
            .FirstOrDefaultAsync(s => s.Subdomain == subdomain);

    public async Task<Shop?> GetShopByOwnerIdAsync(Guid ownerId) =>
        await GetShopQuery()
            .FirstOrDefaultAsync(s => s.OwnerUserId == ownerId);

    public async Task<(IEnumerable<Shop> Shops, int TotalCount)> GetPaginatedShopsAsync(int pageNumber, int pageSize)
    {
        var query = GetShopQuery();
        var totalCount = await query.CountAsync();

        var shops = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (shops, totalCount);
    }

}