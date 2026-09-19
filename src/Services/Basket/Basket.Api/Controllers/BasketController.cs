using System.Security.Claims;
using Basket.Application.Baskets.Dtos;
using Basket.Domain.Entities;
using Basket.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers;

[ApiController]
[Route("api/basket")]
[Authorize]
public sealed class BasketController : ControllerBase
{
    private readonly IBasketRepository _repo;
    public BasketController(IBasketRepository repo) => _repo = repo;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var basket = await _repo.GetAsync(CurrentUserId, ct) ?? new ShoppingBasket { UserId = CurrentUserId };
        return Ok(basket);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddItemDto dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var basket = await _repo.GetAsync(userId, ct) ?? new ShoppingBasket { UserId = userId };

        var existing = basket.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is null)
            basket.Items.Add(new BasketItem { ProductId = dto.ProductId, ProductName = dto.ProductName, UnitPrice = dto.UnitPrice, Quantity = dto.Quantity });
        else
            existing.Quantity += dto.Quantity;

        await _repo.UpdateAsync(basket, ct);
        return Ok(basket);
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, UpdateQuantityDto dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var basket = await _repo.GetAsync(userId, ct);
        if (basket is null) return NotFound();

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return NotFound();

        if (dto.Quantity <= 0) basket.Items.Remove(item);
        else item.Quantity = dto.Quantity;

        await _repo.UpdateAsync(basket, ct);
        return Ok(basket);
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var basket = await _repo.GetAsync(userId, ct);
        if (basket is null) return NotFound();

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null) basket.Items.Remove(item);

        await _repo.UpdateAsync(basket, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _repo.DeleteAsync(CurrentUserId, ct);
        return NoContent();
    }
}
