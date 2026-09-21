using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Dtos;
using Storage.Infrastructure.Data;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly StorageDbContext _db;
    public NotificationsController(StorageDbContext db) => _db = db;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ===== GET: لیست اعلان‌های من =====
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool? unreadOnly, CancellationToken ct)
    {
        var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == UserId);

        if (unreadOnly == true)
            q = q.Where(n => !n.IsRead);

        var items = await q.OrderByDescending(n => n.CreatedAt).Take(100)
            .Select(n => new NotificationDto(
                n.Id, n.Title, n.Message, n.Severity.ToString(),
                n.IsRead, n.RelatedFileId, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);

        return Ok(items);
    }

    // ===== GET: تعداد نخوانده =====
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        var count = await _db.Notifications.CountAsync(n => n.UserId == UserId && !n.IsRead, ct);
        return Ok(new { count });
    }

    // ===== POST: خوانده شدن =====
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId, ct);
        if (n is null) return NotFound();
        n.MarkRead();
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "خوانده شد" });
    }

    // ===== POST: همه خوانده =====
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == UserId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread) n.MarkRead();
        await _db.SaveChangesAsync(ct);

        return Ok(new { marked = unread.Count });
    }

    // ===== DELETE: حذف =====
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId, ct);
        if (n is null) return NotFound();
        _db.Notifications.Remove(n);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
