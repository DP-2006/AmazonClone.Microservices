using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data;

public sealed class IdentityDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }

    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== Users =====
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        // ===== Roles و جداول Identity =====
        builder.Entity<ApplicationRole>(entity => entity.ToTable("roles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(entity => entity.ToTable("user_roles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(entity => entity.ToTable("user_claims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(entity => entity.ToTable("user_logins"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(entity => entity.ToTable("user_tokens"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(entity => entity.ToTable("role_claims"));

        // ===== OtpCodes =====
        builder.Entity<OtpCode>(entity =>
        {
            entity.ToTable("otp_codes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            // Index برای جستجوی سریع
            entity.HasIndex(x => new { x.Email, x.CodeHash, x.IsUsed });
            entity.HasIndex(x => x.ExpiresAt); // برای پاکسازی کدهای منقضی
        });
    }
}
