using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Review.Application.Reviews.Dtos;
using Review.Domain.Entities;
using Review.Infrastructure.Data;

namespace Review.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly ReviewDbContext _db;
    public ReviewsController(ReviewDbContext db) => _db = db;

    private Guid? CurrentUserId
    {
        get
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var g) ? g : null;
        }
    }
    private string CurrentUserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر ناشناس";

    // ===== GET: reviews of a product =====
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
    {
        var all = await _db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        var roots = all.Where(r => r.ParentReviewId == null).Select(r => MapRecursive(r, all)).ToList();
        return Ok(roots);
    }

    // ===== GET: TOP comments for many products (فیچر ۵) =====
    [HttpPost("top-by-products")]
    public async Task<IActionResult> TopByProducts([FromBody] Guid[] productIds, CancellationToken ct)
    {
        if (productIds == null || productIds.Length == 0)
            return Ok(new Dictionary<Guid, List<ReviewDto>>());

        var all = await _db.ProductReviews.AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId) && !r.IsDeleted && r.ParentReviewId == null)
            .OrderByDescending(r => r.LikeCount)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var grouped = all
            .GroupBy(r => r.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.Take(3).Select(r => MapRecursive(r, all)).ToList());

        return Ok(grouped);
    }

    private static ReviewDto MapRecursive(ProductReview r, List<ProductReview> all) =>
        new(r.Id, r.ProductId, r.UserId, r.UserName, r.Rating, r.Title, r.Comment,
            r.ParentReviewId, r.LikeCount, r.DislikeCount, r.CreatedAt, r.UpdatedAt,
            all.Where(c => c.ParentReviewId == r.Id).Select(c => MapRecursive(c, all)).ToList());

    // ===== GET: rating summary =====
    [HttpGet("product/{productId:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid productId, CancellationToken ct)
    {
        var ratings = await _db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && !r.IsDeleted && r.ParentReviewId == null)
            .Select(r => r.Rating).ToListAsync(ct);
        if (ratings.Count == 0) return Ok(new RatingSummaryDto(productId, 0, 0, 0, 0, 0, 0, 0));
        return Ok(new RatingSummaryDto(productId, Math.Round(ratings.Average(), 2), ratings.Count,
            ratings.Count(r => r == 1), ratings.Count(r => r == 2), ratings.Count(r => r == 3),
            ratings.Count(r => r == 4), ratings.Count(r => r == 5)));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateReviewDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var userId = CurrentUserId.Value;
        if (dto.ParentReviewId is null)
        {
            var exists = await _db.ProductReviews.AnyAsync(r =>
                r.ProductId == dto.ProductId && r.UserId == userId
                && r.ParentReviewId == null && !r.IsDeleted, ct);
            if (exists) return Conflict("شما قبلاً نظر داده‌اید.");
        }
        var review = new ProductReview(dto.ProductId, userId, CurrentUserName, dto.Rating, dto.Title, dto.Comment, dto.ParentReviewId);
        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = review.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateReviewDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var review = await _db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (review is null) return NotFound();
        if (review.UserId != CurrentUserId) return Forbid();
        review.Update(dto.Rating, dto.Title, dto.Comment);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var review = await _db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (review is null) return NotFound();
        if (review.UserId != CurrentUserId) return Forbid();
        review.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/react")]
    [Authorize]
    public async Task<IActionResult> React(Guid id, ReactDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var userId = CurrentUserId.Value;
        var newType = dto.Type == "like" ? ReactionType.Like : ReactionType.Dislike;
        var review = await _db.ProductReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (review is null) return NotFound();
        var existing = await _db.ReviewReactions.FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == userId, ct);
        if (existing is null)
        {
            _db.ReviewReactions.Add(new ReviewReaction(id, userId, newType));
            if (newType == ReactionType.Like) review.IncrementLike(); else review.IncrementDislike();
        }
        else if (existing.Type == newType)
        {
            _db.ReviewReactions.Remove(existing);
            if (newType == ReactionType.Like) review.DecrementLike(); else review.DecrementDislike();
        }
        else
        {
            existing.Switch(newType);
            if (newType == ReactionType.Like) { review.IncrementLike(); review.DecrementDislike(); }
            else { review.IncrementDislike(); review.DecrementLike(); }
        }
        await _db.SaveChangesAsync(ct);
        return Ok(new { likes = review.LikeCount, dislikes = review.DislikeCount });
    }

    [HttpPost("{id:guid}/report")]
    [Authorize]
    public async Task<IActionResult> Report(Guid id, ReportReviewDto dto, CancellationToken ct)
    {
        if (CurrentUserId is null) return Unauthorized();
        var exists = await _db.ProductReviews.AnyAsync(r => r.Id == id, ct);
        if (!exists) return NotFound();
        var already = await _db.ReviewReports.AnyAsync(r => r.ReviewId == id && r.ReporterUserId == CurrentUserId, ct);
        if (already) return Conflict("قبلاً گزارش داده‌اید.");
        _db.ReviewReports.Add(new ReviewReport(id, CurrentUserId.Value, dto.Reason));
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "گزارش ثبت شد." });
    }
}
