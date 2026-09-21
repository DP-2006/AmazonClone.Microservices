namespace Review.Domain.Entities;

public sealed class ReviewReport
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReviewId { get; private set; }
    public Guid ReporterUserId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    private ReviewReport() { }
    public ReviewReport(Guid reviewId, Guid reporterUserId, string reason)
    { ReviewId = reviewId; ReporterUserId = reporterUserId; Reason = reason?.Trim() ?? string.Empty; }
}
