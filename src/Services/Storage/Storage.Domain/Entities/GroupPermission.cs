namespace Storage.Domain.Entities;

public sealed class GroupPermission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GroupId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime GrantedAt { get; private set; } = DateTime.UtcNow;
    public Guid? GrantedByUserId { get; private set; }

    private GroupPermission() { }

    public GroupPermission(Guid groupId, Guid permissionId, Guid? grantedBy = null)
    {
        GroupId = groupId;
        PermissionId = permissionId;
        GrantedByUserId = grantedBy;
    }
}
