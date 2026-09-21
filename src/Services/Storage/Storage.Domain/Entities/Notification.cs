namespace Storage.Domain.Entities;

public enum NotificationSeverity { Info = 1, Warning = 2, Critical = 3 }

public sealed class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationSeverity Severity { get; private set; } = NotificationSeverity.Info;
    public bool IsRead { get; private set; }
    public Guid? RelatedFileId { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public Notification(Guid userId, string title, string message,
        NotificationSeverity severity = NotificationSeverity.Info,
        Guid? relatedFileId = null, Guid? createdBy = null)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Severity = severity;
        RelatedFileId = relatedFileId;
        CreatedByUserId = createdBy;
    }

    public void MarkRead() { IsRead = true; ReadAt = DateTime.UtcNow; }
}
