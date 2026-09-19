namespace Orders.Application.Orders.Dtos;

public sealed record OrderItemDto(Guid ProductId, Guid? VariantId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record AddressDto(string Street, string City, string State, string PostalCode, string Country);

public sealed record CreateOrderDto(List<OrderItemDto> Items, AddressDto Address, string Currency = "USD");
