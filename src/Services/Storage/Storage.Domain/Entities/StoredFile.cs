namespace Storage.Domain.Entities;

public sealed class StoredFile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public Guid? FolderId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsPublic { get; private set; }
    public int DownloadCount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Folder? Folder { get; private set; }
    public ICollection<FileShare> Shares { get; private set; } = new List<FileShare>();

    private StoredFile() { }

    public StoredFile(Guid ownerId, string ownerName, string fileName, string originalName,
        string fileUrl, long fileSize, string mimeType, string extension, Guid? folderId = null)
    {
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId required.");
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName required.");
        if (fileSize < 0) throw new ArgumentException("FileSize cannot be negative.");

        OwnerId = ownerId;
        OwnerName = ownerName;
        FileName = fileName;
        OriginalName = originalName;
        FileUrl = fileUrl;
        FileSize = fileSize;
        MimeType = mimeType;
        Extension = extension;
        FolderId = folderId;
    }

    public void UpdateDescription(string? desc) { Description = desc; UpdatedAt = DateTime.UtcNow; }
    public void SetPublic(bool pub) { IsPublic = pub; UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; UpdatedAt = DateTime.UtcNow; }
    public void IncrementDownload() { DownloadCount++; }
    public void SetFolder(Guid? folderId) { FolderId = folderId; UpdatedAt = DateTime.UtcNow; }
}
