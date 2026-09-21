using System.Security.Claims;
using Catalog.Application.Products.Dtos;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly CatalogDbContext _db;

    public ProductsController(CatalogDbContext db) => _db = db;

    private Guid? CurrentUserId
    {
        get
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var g) ? g : null;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductDto(x.Id, x.CategoryId, x.SellerId, x.Name, x.Slug,
                x.Description, x.Price, x.Currency, x.IsActive, 0, 0, x.CreatedAt))
            .ToListAsync(ct);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto(x.Id, x.CategoryId, x.SellerId, x.Name, x.Slug,
                x.Description, x.Price, x.Currency, x.IsActive, 0, 0, x.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        if (User.FindFirstValue(ClaimTypes.Role) != "Seller")
            return Forbid("فقط فروشندگان می‌توانند محصول اضافه کنند.");

        if (await _db.Products.AnyAsync(x => x.Slug == dto.Slug, ct))
            return Conflict("The product slug already exists.");

        if (!await _db.Categories.AnyAsync(x => x.Id == dto.CategoryId, ct))
            return BadRequest("Category not found.");

        var product = new Product(dto.CategoryId, dto.Name, dto.Slug,
            dto.Description, dto.Price, dto.Currency, CurrentUserId);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new ProductDto(product.Id, product.CategoryId, product.SellerId,
                product.Name, product.Slug, product.Description, product.Price,
                product.Currency, product.IsActive, 0, 0, product.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return NotFound();
        if (product.SellerId != CurrentUserId) return Forbid();

        product.Update(dto.Name, dto.Description, dto.Price);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return NotFound();
        if (product.SellerId != CurrentUserId) return Forbid();

        product.Deactivate();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("my-products")]
    [Authorize]
    public async Task<IActionResult> GetMyProducts(CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();

        var products = await _db.Products
            .AsNoTracking()
            .Where(x => x.SellerId == CurrentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductDto(x.Id, x.CategoryId, x.SellerId, x.Name, x.Slug,
                x.Description, x.Price, x.Currency, x.IsActive, 0, 0, x.CreatedAt))
            .ToListAsync(ct);

        return Ok(products);
    }
}
