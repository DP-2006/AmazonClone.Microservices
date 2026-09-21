namespace Chat.Domain.Entities;

public sealed class ChatReport
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MessageId { get; private set; }
    public Guid ReporterId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ChatReport() { }

    public ChatReport(Guid messageId, Guid reporterId, string reason)
    {
        MessageId = messageId;
        ReporterId = reporterId;
        Reason = reason?.Trim() ?? string.Empty;
    }
}
