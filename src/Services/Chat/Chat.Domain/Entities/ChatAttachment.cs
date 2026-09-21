namespace Chat.Domain.Entities;

public sealed class ChatAttachment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ChatAttachment() { }

    public ChatAttachment(Guid messageId, string fileName, string fileUrl, long size, string mimeType)
    {
        MessageId = messageId;
        FileName = fileName;
        FileUrl = fileUrl;
        FileSize = size;
        MimeType = mimeType;
    }
}
