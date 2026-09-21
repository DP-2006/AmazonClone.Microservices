namespace Catalog.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    Guid? SellerId,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    string Currency,
    bool IsActive,
    double AverageRating,
    int ReviewCount,
    DateTime CreatedAt);
