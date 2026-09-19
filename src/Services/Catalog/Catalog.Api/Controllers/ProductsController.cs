using Catalog.Application.Products.Dtos;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly CatalogDbContext _db;
    public ProductsController(CatalogDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await _db.Products.AsNoTracking().Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProductDto(x.Id, x.CategoryId, x.Name, x.Slug, x.Description, x.Price, x.Currency, x.IsActive, x.CreatedAt))
            .ToListAsync(ct);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new ProductDto(x.Id, x.CategoryId, x.Name, x.Slug, x.Description, x.Price, x.Currency, x.IsActive, x.CreatedAt))
            .FirstOrDefaultAsync(ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct)
    {
        if (await _db.Products.AnyAsync(x => x.Slug == dto.Slug, ct))
            return Conflict("The product slug already exists.");
        if (!await _db.Categories.AnyAsync(x => x.Id == dto.CategoryId, ct))
            return BadRequest("Category not found.");
        var product = new Product(dto.CategoryId, dto.Name, dto.Slug, dto.Description, dto.Price, dto.Currency);
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new ProductDto(product.Id, product.CategoryId, product.Name, product.Slug, product.Description, product.Price, product.Currency, product.IsActive, product.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto dto, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return NotFound();
        product.Update(dto.Name, dto.Description, dto.Price);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return NotFound();
        product.Deactivate();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
