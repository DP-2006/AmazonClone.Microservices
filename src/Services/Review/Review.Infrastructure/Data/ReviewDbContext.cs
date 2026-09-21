using Microsoft.EntityFrameworkCore;
using Review.Domain.Entities;

namespace Review.Infrastructure.Data;

public sealed class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ReviewReaction> ReviewReactions => Set<ReviewReaction>();
    public DbSet<ReviewReport> ReviewReports => Set<ReviewReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ProductReview>(e =>
        {
            e.ToTable("product_reviews");
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Comment).HasMaxLength(3000).IsRequired();
            e.HasIndex(x => x.ProductId);
            e.HasMany(x => x.Replies).WithOne().HasForeignKey(x => x.ParentReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Reactions).WithOne().HasForeignKey(x => x.ReviewId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Reports).WithOne().HasForeignKey(x => x.ReviewId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<ReviewReaction>(e =>
        {
            e.ToTable("review_reactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<int>();
            e.HasIndex(x => new { x.ReviewId, x.UserId }).IsUnique();
        });
        b.Entity<ReviewReport>(e =>
        {
            e.ToTable("review_reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            e.HasIndex(x => new { x.ReviewId, x.ReporterUserId }).IsUnique();
        });
    }
}
