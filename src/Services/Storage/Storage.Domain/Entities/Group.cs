namespace Storage.Domain.Entities;

public sealed class Group
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsSystemGroup { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();
    public ICollection<GroupPermission> Permissions { get; private set; } = new List<GroupPermission>();

    private Group() { }

    public Group(string name, Guid createdBy, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");
        Name = name.Trim();
        CreatedByUserId = createdBy;
        Description = description;
        IsSystemGroup = isSystem;
    }

    public void Rename(string newName) { Name = newName.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void UpdateDescription(string? desc) { Description = desc; UpdatedAt = DateTime.UtcNow; }
}
