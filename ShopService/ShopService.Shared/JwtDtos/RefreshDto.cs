namespace ShopService.Shared.JwtDtos;

public record RefreshDto(string AccessToken, string RefreshToken);