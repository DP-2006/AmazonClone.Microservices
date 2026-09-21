namespace Storage.Domain.Entities;

public sealed class Folder
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid? ParentFolderId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Folder? Parent { get; private set; }
    public ICollection<Folder> Children { get; private set; } = new List<Folder>();
    public ICollection<StoredFile> Files { get; private set; } = new List<StoredFile>();

    private Folder() { }

    public Folder(Guid ownerId, string name, Guid? parentId = null)
    {
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");

        OwnerId = ownerId;
        Name = name.Trim();
        ParentFolderId = parentId;
    }

    public void Rename(string newName) { Name = newName.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; UpdatedAt = DateTime.UtcNow; }
}
