using ShopService.Core.Entities;
using ShopService.Core.Interfaces;
using ShopService.Infrastructure.Interfaces.Base;
using ShopService.Infrastructure.Interfaces.Entities;
using ShopService.Shared.Dtos;

namespace ShopService.Application.Services;

using AutoMapper;

public class ShopService(
    IUnitOfWork unitOfWork,
    IShopRepository shopRepository,
    IMapper mapper
) : IShopService
{
    public async Task<GetShopDto> CreateShopAsync(CreateShopDto dto, Guid ownerUserId)
    {
        var existingShop = await shopRepository.GetShopBySubdomainAsync(dto.Subdomain);
        if (existingShop != null)
            throw new InvalidOperationException("Магазин с таким поддоменом уже существует.");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var shopEntity = mapper.Map<Shop>(dto);
            // Id генерируется автоматически в конструкторе / инициализации свойства
            shopEntity.OwnerUserId = ownerUserId;
            shopEntity.CreatedAt = DateTime.UtcNow;
            shopEntity.IsActive = false;

            await shopRepository.InsertAsync(shopEntity);

            await unitOfWork.CommitTransactionAsync();
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<GetShopDto>(shopEntity);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<GetShopDto?> GetShopByIdAsync(Guid id) =>
        mapper.Map<GetShopDto>(await shopRepository.GetShopByIdAsync(id));

    public async Task<GetShopDto?> GetShopBySubdomainAsync(string subdomain) =>
        mapper.Map<GetShopDto>(await shopRepository.GetShopBySubdomainAsync(subdomain));

    public async Task<GetShopDto?> GetShopByOwnerIdAsync(Guid ownerId) =>
        mapper.Map<GetShopDto>(await shopRepository.GetShopByOwnerIdAsync(ownerId));

    public async Task<(IEnumerable<GetShopDto> Shops, int TotalCount)> GetPaginatedShopsAsync(int pageNumber, int pageSize)
    {
        var (shops, totalCount) = await shopRepository.GetPaginatedShopsAsync(pageNumber, pageSize);
        return (mapper.Map<IEnumerable<GetShopDto>>(shops), totalCount);
    }

    public async Task<bool> UpdateShopAsync(Guid id, UpdateShopDto updateShopDto)
    {
        var shopEntity = await shopRepository.GetShopByIdAsync(id);
        if (shopEntity == null)
            return false;

        if (!string.IsNullOrWhiteSpace(updateShopDto.Subdomain) &&
            shopEntity.Subdomain != updateShopDto.Subdomain)
        {
            var existingShop = await shopRepository.GetShopBySubdomainAsync(updateShopDto.Subdomain);
            if (existingShop != null && existingShop.Id != id)
                throw new InvalidOperationException("Магазин с таким поддоменом уже существует.");
        }

        mapper.Map(updateShopDto, shopEntity);
        shopEntity.UpdatedAt = DateTime.UtcNow;

        shopRepository.Update(shopEntity);
        var success = await unitOfWork.SaveChangesAsync();
        return success;
    }

    public async Task<bool> DeleteShopAsync(Guid id)
    {
        var shopEntity = await shopRepository.GetShopByIdAsync(id);
        if (shopEntity == null)
            return false;

        shopRepository.Delete(shopEntity);
        var success = await unitOfWork.SaveChangesAsync();
        return success;
    }
}