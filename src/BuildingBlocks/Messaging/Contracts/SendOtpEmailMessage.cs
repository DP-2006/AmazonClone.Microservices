namespace BuildingBlocks.Messaging.Contracts;

public sealed record SendOtpEmailMessage
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string OtpCode { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}
