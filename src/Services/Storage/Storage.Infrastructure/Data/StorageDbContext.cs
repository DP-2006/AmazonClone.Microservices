using Microsoft.EntityFrameworkCore;
using Storage.Domain.Entities;
using FileShareEntity = Storage.Domain.Entities.FileShare;

namespace Storage.Infrastructure.Data;

public sealed class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options) { }

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<FileShareEntity> FileShares => Set<FileShareEntity>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<StorageSettings> StorageSettings => Set<StorageSettings>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<StoredFile>(e =>
        {
            e.ToTable("stored_files");
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(300).IsRequired();
            e.Property(x => x.OriginalName).HasMaxLength(300).IsRequired();
            e.Property(x => x.FileUrl).HasMaxLength(1000).IsRequired();
            e.Property(x => x.MimeType).HasMaxLength(150).IsRequired();
            e.Property(x => x.Extension).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.FolderId);
            e.HasIndex(x => new { x.OwnerId, x.IsDeleted });

            e.HasOne(x => x.Folder).WithMany(f => f.Files)
                .HasForeignKey(x => x.FolderId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Shares).WithOne()
                .HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Folder>(e =>
        {
            e.ToTable("folders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => new { x.OwnerId, x.ParentFolderId });

            e.HasOne(x => x.Parent).WithMany(f => f.Children)
                .HasForeignKey(x => x.ParentFolderId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<FileShareEntity>(e =>
        {
            e.ToTable("file_shares");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(1000);
            e.HasIndex(x => new { x.FileId, x.SharedWithUserId }).IsUnique();
            e.HasIndex(x => x.SharedWithUserId);
        });

        b.Entity<Group>(e =>
        {
            e.ToTable("groups");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => x.Name).IsUnique();

            e.HasMany(x => x.Members).WithOne()
                .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Permissions).WithOne()
                .HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<GroupMember>(e =>
        {
            e.ToTable("group_members");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
            e.HasIndex(x => x.UserId);
        });

        b.Entity<Permission>(e =>
        {
            e.ToTable("permissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.Code).IsUnique();
        });

        b.Entity<GroupPermission>(e =>
        {
            e.ToTable("group_permissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.GroupId, x.PermissionId }).IsUnique();
        });

        b.Entity<UserPermission>(e =>
        {
            e.ToTable("user_permissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.PermissionId }).IsUnique();
            e.HasIndex(x => x.UserId);
        });

        b.Entity<ActivityLog>(e =>
        {
            e.ToTable("activity_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            e.Property(x => x.ResourceType).HasMaxLength(100);
            e.Property(x => x.IpAddress).HasMaxLength(50);
            e.Property(x => x.UserAgent).HasMaxLength(500);
            e.Property(x => x.Metadata).HasMaxLength(4000);
            e.Property(x => x.Type).HasConversion<int>();
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Type);
        });

        b.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Severity).HasConversion<int>();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.UserId, x.IsRead });
        });

        b.Entity<StorageSettings>(e =>
        {
            e.ToTable("storage_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.AllowedExtensions).HasColumnType("text[]");
            e.Property(x => x.BlockedExtensions).HasColumnType("text[]");
        });
    }
}
