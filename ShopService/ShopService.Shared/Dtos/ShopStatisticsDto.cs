namespace ShopService.Shared.Dtos;

public record ShopStatisticsDto(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    int ProductsSold,
    DateTime PeriodStart,
    DateTime PeriodEnd
);