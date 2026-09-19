using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Orders.Dtos;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly OrdersDbContext _db;
    public PaymentsController(OrdersDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentDto dto, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId, ct);
        if (order is null) return NotFound("Order not found.");

        var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpper();
        var payment = new Payment(order.Id, dto.Provider, transactionId, order.TotalAmount);
        payment.MarkSucceeded();
        order.MarkPaid();

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return Ok(new { payment.Id, payment.Status, payment.TransactionId, payment.Amount });
    }
}
