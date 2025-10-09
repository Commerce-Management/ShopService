namespace ShopService.Shared.Dtos;


public record UpdateShopDto
(
    string Name,
    string Description,
    string ContactEmail,
    string ContactPhone,
    string Subdomain
);