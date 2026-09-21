namespace Review.Domain.Entities;

public sealed class ProductReview
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public Guid? ParentReviewId { get; private set; }
    public int LikeCount { get; private set; }
    public int DislikeCount { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<ProductReview> Replies { get; private set; } = new List<ProductReview>();
    public ICollection<ReviewReaction> Reactions { get; private set; } = new List<ReviewReaction>();
    public ICollection<ReviewReport> Reports { get; private set; } = new List<ReviewReport>();
    private ProductReview() { }
    public ProductReview(Guid productId, Guid userId, string userName, int rating, string? title, string comment, Guid? parentReviewId = null)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId required.");
        if (userId == Guid.Empty) throw new ArgumentException("UserId required.");
        if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be 1..5.");
        ProductId = productId; UserId = userId; UserName = userName;
        Rating = rating; Title = title?.Trim(); Comment = comment?.Trim() ?? string.Empty;
        ParentReviewId = parentReviewId;
    }
    public void Update(int rating, string? title, string comment)
    {
        if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be 1..5.");
        Rating = rating; Title = title?.Trim(); Comment = comment?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SoftDelete() { IsDeleted = true; UpdatedAt = DateTime.UtcNow; }
    public void IncrementLike() { LikeCount++; UpdatedAt = DateTime.UtcNow; }
    public void DecrementLike() { if (LikeCount > 0) LikeCount--; UpdatedAt = DateTime.UtcNow; }
    public void IncrementDislike() { DislikeCount++; UpdatedAt = DateTime.UtcNow; }
    public void DecrementDislike() { if (DislikeCount > 0) DislikeCount--; UpdatedAt = DateTime.UtcNow; }
}
