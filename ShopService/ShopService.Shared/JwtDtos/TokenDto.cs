namespace ShopService.Shared.JwtDtos;

public record TokenDto(string AccessToken, string RefreshToken);