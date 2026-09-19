namespace Basket.Application.Baskets.Dtos;

public sealed record AddItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public sealed record UpdateQuantityDto(int Quantity);
