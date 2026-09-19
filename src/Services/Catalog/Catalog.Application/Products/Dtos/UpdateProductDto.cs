namespace Catalog.Application.Products.Dtos;

public sealed record UpdateProductDto(string Name, string Description, decimal Price);
