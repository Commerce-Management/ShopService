using ShopService.Core.Entities;
using ShopService.Infrastructure.Interfaces.Base;

namespace ShopService.Infrastructure.Interfaces.Entities;

public interface IShopRepository : IRepository<Shop>
{
    public Task<Shop?> GetShopByIdAsync(Guid id);
    public Task<Shop?> GetShopBySubdomainAsync(string subdomain);
    public Task<Shop?> GetShopByOwnerIdAsync(Guid ownerId);
    public Task<IEnumerable<Shop>> GetAllShopsAsync();
    public Task<(IEnumerable<Shop> Shops, int TotalCount)> GetPaginatedShopsAsync(int pageNumber, int pageSize);
    public IQueryable<Shop> GetQueryableEntities();
} 