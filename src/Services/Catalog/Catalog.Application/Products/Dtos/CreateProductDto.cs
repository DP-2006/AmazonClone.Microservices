namespace Catalog.Application.Products.Dtos;

public sealed record CreateProductDto(Guid CategoryId, string Name, string Slug, string Description, decimal Price, string Currency);
