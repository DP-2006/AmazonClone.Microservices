namespace Storage.Domain.Entities;

public sealed class UserPermission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime GrantedAt { get; private set; } = DateTime.UtcNow;
    public Guid? GrantedByUserId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private UserPermission() { }

    public UserPermission(Guid userId, Guid permissionId, Guid? grantedBy = null, DateTime? expiresAt = null)
    {
        UserId = userId;
        PermissionId = permissionId;
        GrantedByUserId = grantedBy;
        ExpiresAt = expiresAt;
    }
}
