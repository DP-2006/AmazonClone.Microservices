using System.Security.Claims;
using Chat.Application.Chats.Dtos;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Chat.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Chat.Api.Hubs;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly FileStorageService _files;
    private readonly IHubContext<ChatHub> _hub;

    public ChatController(ChatDbContext db, FileStorageService files, IHubContext<ChatHub> hub)
    {
        _db = db;
        _files = files;
        _hub = hub;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => User.IsInRole("Admin");
    private bool IsSeller => User.IsInRole("Seller");

    // ===== لیست اتاق‌های من =====
    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms(CancellationToken ct)
    {
        IQueryable<ChatRoom> q = _db.ChatRooms.AsNoTracking();

        if (!IsAdmin)
            q = q.Where(r => r.BuyerId == UserId || r.SellerId == UserId);

        var rooms = await q.OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt).ToListAsync(ct);

        var result = new List<ChatRoomDto>();
        foreach (var r in rooms)
        {
            var last = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.RoomId == r.Id && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.Content ?? "[attachment]")
                .FirstOrDefaultAsync(ct);

            var unread = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.RoomId == r.Id && m.SenderId != UserId && m.ReadAt == null && !m.IsDeleted)
                .CountAsync(ct);

            result.Add(new ChatRoomDto(r.Id, r.BuyerId, r.SellerId,
                r.SellerChatEnabled, r.LastMessageAt, r.CreatedAt, last, unread));
        }

        return Ok(result);
    }

    // ===== ساخت اتاق =====
    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom(CreateRoomDto dto, CancellationToken ct)
    {
        if (UserId == dto.SellerId) return BadRequest("نمی‌توانید با خودتان چت کنید.");

        // خریدار پیام می‌ده، پس BuyerId = UserId
        var buyerId = IsSeller ? dto.SellerId : UserId;
        var sellerId = IsSeller ? UserId : dto.SellerId;

        var existing = await _db.ChatRooms.AsNoTracking()
            .FirstOrDefaultAsync(r => r.BuyerId == buyerId && r.SellerId == sellerId, ct);

        if (existing is not null) return Ok(new { id = existing.Id });

        var room = new ChatRoom(buyerId, sellerId);
        _db.ChatRooms.Add(room);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = room.Id });
    }

    // ===== جزئیات اتاق =====
    [HttpGet("rooms/{id:guid}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var room = await _db.ChatRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return NotFound();
        if (!room.IsParticipant(UserId) && !IsAdmin) return Forbid();

        return Ok(new ChatRoomDto(room.Id, room.BuyerId, room.SellerId,
            room.SellerChatEnabled, room.LastMessageAt, room.CreatedAt, null, 0));
    }

    // ===== پیام‌ها =====
    [HttpGet("rooms/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var room = await _db.ChatRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return NotFound();
        if (!room.IsParticipant(UserId) && !IsAdmin) return Forbid();

        var total = await _db.ChatMessages.CountAsync(m => m.RoomId == id && !m.IsDeleted, ct);
        var msgs = await _db.ChatMessages.AsNoTracking()
            .Where(m => m.RoomId == id && !m.IsDeleted)
            .Include(m => m.Attachments)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        var result = msgs.Select(m => new ChatMessageDto(
            m.Id, m.RoomId, m.SenderId, m.SenderName, m.SenderRole.ToString(),
            m.Content, m.Type.ToString(), m.IsDeleted, m.IsFlagged,
            m.CreatedAt, m.ReadAt,
            m.Attachments.Select(a => new ChatAttachmentDto(
                a.Id, a.FileName, a.FileUrl, a.FileSize, a.MimeType)).ToList()
        )).ToList();

        return Ok(new MessagePageDto(result, total, page, pageSize));
    }

    // ===== ارسال پیام =====
    [HttpPost("rooms/{id:guid}/messages")]
    public async Task<IActionResult> Send(Guid id, SendMessageDto dto, CancellationToken ct)
    {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return NotFound();
        if (!room.IsParticipant(UserId)) return Forbid();
        if (!room.SellerChatEnabled && !IsSeller)
            return BadRequest("فروشنده چت را غیرفعال کرده است.");

        var role = IsSeller ? SenderRole.Seller : SenderRole.Buyer;
        var type = Enum.Parse<MessageType>(dto.Type);

        var msg = new ChatMessage(room.Id, UserId, UserName, role, dto.Content, type);
        _db.ChatMessages.Add(msg);
        room.TouchMessage();
        await _db.SaveChangesAsync(ct);

        var payload = new ChatMessageDto(msg.Id, msg.RoomId, msg.SenderId, msg.SenderName,
            msg.SenderRole.ToString(), msg.Content, msg.Type.ToString(),
            msg.IsDeleted, msg.IsFlagged, msg.CreatedAt, msg.ReadAt, new());

        await _hub.Clients.Group($"room_{id}").SendAsync("NewMessage", payload);
        await _hub.Clients.Group("admins").SendAsync("NewMessage", payload);

        return Ok(payload);
    }

    // ===== آپلود فایل =====
    [HttpPost("rooms/{id:guid}/upload")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return NotFound();
        if (!room.IsParticipant(UserId)) return Forbid();
        if (file is null) return BadRequest("فایل نیست");

        var (category, isValid, error) = _files.Validate(file);
        if (!isValid) return BadRequest(error);

        var (fileName, fileUrl, size, mime) = await _files.SaveAsync(file, category, ct);

        var msgType = category switch
        {
            "images" => MessageType.Image,
            "videos" => MessageType.Video,
            _ => MessageType.File
        };

        var role = IsSeller ? SenderRole.Seller : SenderRole.Buyer;
        var msg = new ChatMessage(room.Id, UserId, UserName, role, null, msgType);

        _db.ChatMessages.Add(msg);
        _db.ChatAttachments.Add(new ChatAttachment(msg.Id, fileName, fileUrl, size, mime));
        room.TouchMessage();
        await _db.SaveChangesAsync(ct);

        var payload = new ChatMessageDto(msg.Id, msg.RoomId, msg.SenderId, msg.SenderName,
            msg.SenderRole.ToString(), msg.Content, msg.Type.ToString(),
            msg.IsDeleted, msg.IsFlagged, msg.CreatedAt, msg.ReadAt,
            new List<ChatAttachmentDto> { new(Guid.Empty, fileName, fileUrl, size, mime) });

        await _hub.Clients.Group($"room_{id}").SendAsync("NewMessage", payload);
        await _hub.Clients.Group("admins").SendAsync("NewMessage", payload);

        return Ok(payload);
    }

    // ===== toggle chat =====
    [HttpPost("rooms/{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (room is null) return NotFound();
        if (room.SellerId != UserId) return Forbid("فقط فروشنده می‌تواند تغییر دهد");

        if (room.SellerChatEnabled) room.DisableChat();
        else room.EnableChat();

        await _db.SaveChangesAsync(ct);
        return Ok(new { enabled = room.SellerChatEnabled });
    }

    // ===== گزارش پیام =====
    [HttpPost("messages/{id:guid}/report")]
    public async Task<IActionResult> Report(Guid id, ReportMessageDto dto, CancellationToken ct)
    {
        var msg = await _db.ChatMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg is null) return NotFound();

        var already = await _db.ChatReports.AnyAsync(r => r.MessageId == id && r.ReporterId == UserId, ct);
        if (already) return Conflict("قبلاً گزارش کرده‌اید");

        _db.ChatReports.Add(new ChatReport(id, UserId, dto.Reason));
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "گزارش ثبت شد" });
    }

    // ===== soft delete =====
    [HttpDelete("messages/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (msg is null) return NotFound();
        if (msg.SenderId != UserId && !IsAdmin) return Forbid();

        msg.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
