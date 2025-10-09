namespace ShopService.Shared.Dtos;

public record GetShopDto(
    string Id,
    string Name,
    string Description,
    string ContactEmail,
    string ContactPhone,
    string Subdomain,
    string OwnerUserId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
); 