namespace Storage.Domain.Entities;

public enum ActivityType
{
    Login = 1,
    Logout = 2,
    Upload = 3,
    Download = 4,
    Delete = 5,
    View = 6,
    Edit = 7,
    Share = 8,
    Move = 9,
    CreateFolder = 10,
    DeleteFolder = 11,
    PermissionChange = 12,
    UserBlocked = 13,
    UserUnblocked = 14,
    PasswordChange = 15,
    Report = 16
}

public sealed class ActivityLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? UserId { get; private set; }
    public string Username { get; private set; } = "سیستم";
    public ActivityType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ResourceType { get; private set; }
    public Guid? ResourceId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ActivityLog() { }

    public ActivityLog(Guid? userId, string username, ActivityType type,
        string description, string? resourceType = null, Guid? resourceId = null,
        string? ipAddress = null, string? userAgent = null, string? metadata = null)
    {
        UserId = userId;
        Username = username;
        Type = type;
        Description = description;
        ResourceType = resourceType;
        ResourceId = resourceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Metadata = metadata;
    }
}
