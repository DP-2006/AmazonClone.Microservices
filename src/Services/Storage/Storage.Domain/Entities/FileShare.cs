namespace Storage.Domain.Entities;

public sealed class FileShare
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid FileId { get; private set; }
    public Guid SharedByUserId { get; private set; }
    public Guid SharedWithUserId { get; private set; }
    public string? Message { get; private set; }
    public bool CanDownload { get; private set; } = true;
    public bool CanReshare { get; private set; } = false;
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private FileShare() { }

    public FileShare(Guid fileId, Guid sharedBy, Guid sharedWith, string? message = null,
        bool canDownload = true, bool canReshare = false, DateTime? expiresAt = null)
    {
        FileId = fileId;
        SharedByUserId = sharedBy;
        SharedWithUserId = sharedWith;
        Message = message;
        CanDownload = canDownload;
        CanReshare = canReshare;
        ExpiresAt = expiresAt;
    }
}
