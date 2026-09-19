using Catalog.Application.Categories.Dtos;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CatalogDbContext _db;
    public CategoriesController(CatalogDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var categories = await _db.Categories.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.Id, x.ParentId, x.Name, x.Slug, x.CreatedAt))
            .ToListAsync(ct);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _db.Categories.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CategoryDto(x.Id, x.ParentId, x.Name, x.Slug, x.CreatedAt))
            .FirstOrDefaultAsync(ct);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto, CancellationToken ct)
    {
        if (await _db.Categories.AnyAsync(x => x.Slug == dto.Slug, ct))
            return Conflict("The category slug already exists.");
        if (dto.ParentId.HasValue && !await _db.Categories.AnyAsync(x => x.Id == dto.ParentId, ct))
            return BadRequest("Parent category not found.");
        var category = new Category(dto.Name, dto.Slug, dto.ParentId);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryDto(category.Id, category.ParentId, category.Name, category.Slug, category.CreatedAt));
    }
}
