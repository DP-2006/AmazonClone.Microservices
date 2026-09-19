namespace Catalog.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    private Category() { }
    public Category(string name, string slug, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentId = parentId;
    }
}
