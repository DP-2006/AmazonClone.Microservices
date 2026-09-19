namespace Orders.Application.Orders.Dtos;

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    string OrderNumber,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    List<OrderItemViewDto> Items);

public sealed record OrderItemViewDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
