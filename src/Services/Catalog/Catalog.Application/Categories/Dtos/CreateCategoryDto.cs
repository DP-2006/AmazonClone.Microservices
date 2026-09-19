namespace Catalog.Application.Categories.Dtos;

public sealed record CreateCategoryDto(string Name, string Slug, Guid? ParentId);
