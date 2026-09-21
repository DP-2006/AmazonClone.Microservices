namespace Review.Domain.Entities;

public enum ReactionType { Like = 1, Dislike = 2 }

public sealed class ReviewReaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReviewId { get; private set; }
    public Guid UserId { get; private set; }
    public ReactionType Type { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    private ReviewReaction() { }
    public ReviewReaction(Guid reviewId, Guid userId, ReactionType type)
    { ReviewId = reviewId; UserId = userId; Type = type; }
    public void Switch(ReactionType type) { Type = type; }
}
