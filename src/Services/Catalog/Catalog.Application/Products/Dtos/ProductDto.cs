namespace Catalog.Application.Products.Dtos;

public sealed record ProductDto(Guid Id, Guid CategoryId, string Name, string Slug, string Description, decimal Price, string Currency, bool IsActive, DateTime CreatedAt);
