namespace Orders.Application.Orders.Dtos;

public sealed record CreatePaymentDto(Guid OrderId, string Provider);
