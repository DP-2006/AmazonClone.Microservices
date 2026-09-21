namespace Review.Application.Reviews.Dtos;

public sealed record CreateReviewDto(Guid ProductId, int Rating, string? Title, string Comment, Guid? ParentReviewId);
public sealed record UpdateReviewDto(int Rating, string? Title, string Comment);
public sealed record ReviewDto(Guid Id, Guid ProductId, Guid UserId, string UserName, int Rating, string? Title, string Comment, Guid? ParentReviewId, int LikeCount, int DislikeCount, DateTime CreatedAt, DateTime UpdatedAt, List<ReviewDto> Replies);
public sealed record RatingSummaryDto(Guid ProductId, double AverageRating, int TotalCount, int Star1, int Star2, int Star3, int Star4, int Star5);
public sealed record ReactDto(string Type);
public sealed record ReportReviewDto(string Reason);
