namespace Chat.Domain.Entities;

public enum MessageType { Text = 1, Image = 2, Video = 3, File = 4 }
public enum SenderRole { Buyer = 1, Seller = 2, Admin = 3 }

public sealed class ChatMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RoomId { get; private set; }
    public Guid SenderId { get; private set; }
    public string SenderName { get; private set; } = string.Empty;
    public SenderRole SenderRole { get; private set; }
    public string? Content { get; private set; }
    public MessageType Type { get; private set; } = MessageType.Text;
    public bool IsDeleted { get; private set; }
    public bool IsFlagged { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; private set; }

    public ICollection<ChatAttachment> Attachments { get; private set; } = new List<ChatAttachment>();
    public ICollection<ChatReport> Reports { get; private set; } = new List<ChatReport>();

    private ChatMessage() { }

    public ChatMessage(Guid roomId, Guid senderId, string senderName, SenderRole role,
        string? content, MessageType type = MessageType.Text)
    {
        RoomId = roomId;
        SenderId = senderId;
        SenderName = senderName;
        SenderRole = role;
        Content = content?.Trim();
        Type = type;
    }

    public void MarkRead() { ReadAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; }
    public void Flag() { IsFlagged = true; }
}
