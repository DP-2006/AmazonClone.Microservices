namespace Catalog.Application.Categories.Dtos;

public sealed record CategoryDto(Guid Id, Guid? ParentId, string Name, string Slug, DateTime CreatedAt);
