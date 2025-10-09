using ShopService.Shared.Dtos;

namespace ShopService.Core.Interfaces;

public interface IShopService
{
    Task<GetShopDto> CreateShopAsync(CreateShopDto createShopDto, Guid ownerUserId);
    Task<GetShopDto?> GetShopByIdAsync(Guid id);
    Task<GetShopDto?> GetShopBySubdomainAsync(string subdomain);
    Task<GetShopDto?> GetShopByOwnerIdAsync(Guid ownerId);
    Task<(IEnumerable<GetShopDto> Shops, int TotalCount)> GetPaginatedShopsAsync(int pageNumber, int pageSize);
    Task<bool> UpdateShopAsync(Guid id, UpdateShopDto updateShopDto);
    Task<bool> DeleteShopAsync(Guid id);
} 