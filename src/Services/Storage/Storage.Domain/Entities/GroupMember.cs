namespace Storage.Domain.Entities;

public sealed class GroupMember
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
    public Guid? AddedByUserId { get; private set; }

    private GroupMember() { }

    public GroupMember(Guid groupId, Guid userId, string username, Guid? addedBy = null)
    {
        GroupId = groupId;
        UserId = userId;
        Username = username;
        AddedByUserId = addedBy;
    }
}
