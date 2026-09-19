using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Orders.Dtos;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _db;
    public OrdersController(OrdersDbContext db) => _db = db;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = CurrentUserId;
        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto(o.Id, o.UserId, o.OrderNumber, o.Status.ToString(),
                o.TotalAmount, o.Currency, o.CreatedAt,
                o.Items.Select(i => new OrderItemViewDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()))
            .ToListAsync(ct);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var order = await _db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && o.UserId == userId)
            .Select(o => new OrderDto(o.Id, o.UserId, o.OrderNumber, o.Status.ToString(),
                o.TotalAmount, o.Currency, o.CreatedAt,
                o.Items.Select(i => new OrderItemViewDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()))
            .FirstOrDefaultAsync(ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var order = new Order(userId, orderNumber, dto.Currency);

        foreach (var item in dto.Items)
            order.AddItem(item.ProductId, item.VariantId, item.ProductName, item.UnitPrice, item.Quantity);

        order.SetAddress(dto.Address.Street, dto.Address.City, dto.Address.State, dto.Address.PostalCode, dto.Address.Country);
        order.Confirm();

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            new OrderDto(order.Id, order.UserId, order.OrderNumber, order.Status.ToString(),
                order.TotalAmount, order.Currency, order.CreatedAt,
                order.Items.Select(i => new OrderItemViewDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);
        if (order is null) return NotFound();
        order.Cancel();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
