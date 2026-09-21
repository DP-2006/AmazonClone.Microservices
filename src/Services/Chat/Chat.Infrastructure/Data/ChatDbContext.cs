using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Data;

public sealed class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatAttachment> ChatAttachments => Set<ChatAttachment>();
    public DbSet<ChatReport> ChatReports => Set<ChatReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ChatRoom>(e =>
        {
            e.ToTable("chat_rooms");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BuyerId, x.SellerId }).IsUnique();
            e.HasIndex(x => x.LastMessageAt);
            e.HasMany(x => x.Messages).WithOne()
                .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ChatMessage>(e =>
        {
            e.ToTable("chat_messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.SenderName).HasMaxLength(200).IsRequired();
            e.Property(x => x.SenderRole).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Content).HasMaxLength(4000);
            e.HasIndex(x => x.RoomId);
            e.HasIndex(x => x.CreatedAt);
            e.HasMany(x => x.Attachments).WithOne()
                .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Reports).WithOne()
                .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ChatAttachment>(e =>
        {
            e.ToTable("chat_attachments");
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(300).IsRequired();
            e.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
            e.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        });

        b.Entity<ChatReport>(e =>
        {
            e.ToTable("chat_reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            e.HasIndex(x => new { x.MessageId, x.ReporterId }).IsUnique();
        });
    }
}
