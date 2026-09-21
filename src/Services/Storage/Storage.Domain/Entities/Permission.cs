namespace Storage.Domain.Entities;

public sealed class Permission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Permission() { }

    public Permission(string code, string name, string category, string? description = null)
    {
        Code = code.Trim().ToLowerInvariant();
        Name = name.Trim();
        Category = category.Trim();
        Description = description;
    }
}
