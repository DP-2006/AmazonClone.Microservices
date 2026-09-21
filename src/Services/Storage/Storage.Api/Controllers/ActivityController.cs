using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Dtos;
using Storage.Infrastructure.Data;
using Storage.Infrastructure.Services;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/activity")]
[Authorize]
public sealed class ActivityController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly PermissionService _perm;

    public ActivityController(StorageDbContext db, PermissionService perm)
    {
        _db = db;
        _perm = perm;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    private async Task<bool> HasPerm(string code, CancellationToken ct)
    {
        if (IsAdmin) return true;
        return await _perm.HasPermissionAsync(UserId, code, ct);
    }

    // ===== GET: لیست لاگ‌ها با فیلتر =====
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] ActivityLogFilterDto filter, CancellationToken ct)
    {
        if (!await HasPerm("logs.view", ct)) return Forbid();

        var q = _db.ActivityLogs.AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue)
            q = q.Where(a => a.UserId == filter.UserId);

        if (!string.IsNullOrWhiteSpace(filter.Username))
            q = q.Where(a => a.Username.Contains(filter.Username));

        if (!string.IsNullOrWhiteSpace(filter.Type) &&
            Enum.TryParse<Storage.Domain.Entities.ActivityType>(filter.Type, true, out var type))
            q = q.Where(a => a.Type == type);

        if (filter.FromDate.HasValue)
            q = q.Where(a => a.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            q = q.Where(a => a.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.ResourceType))
            q = q.Where(a => a.ResourceType == filter.ResourceType);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            q = q.Where(a => a.Description.ToLower().Contains(s)
                || a.Username.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 10, 200);

        var items = await q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityLogDto(
                a.Id, a.UserId, a.Username, a.Type.ToString(), a.Description,
                a.ResourceType, a.ResourceId, a.IpAddress, a.UserAgent,
                a.Metadata, a.CreatedAt))
            .ToListAsync(ct);

        return Ok(new PagedResultDto<ActivityLogDto>(items, total, page, pageSize,
            (int)Math.Ceiling(total / (double)pageSize)));
    }

    // ===== GET: جزئیات =====
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("logs.view", ct)) return Forbid();

        var log = await _db.ActivityLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (log is null) return NotFound();

        return Ok(new ActivityLogDto(log.Id, log.UserId, log.Username, log.Type.ToString(),
            log.Description, log.ResourceType, log.ResourceId, log.IpAddress,
            log.UserAgent, log.Metadata, log.CreatedAt));
    }

    // ===== GET: آمار =====
    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        if (!await HasPerm("logs.view", ct)) return Forbid();

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var total = await _db.ActivityLogs.CountAsync(ct);
        var todayCount = await _db.ActivityLogs.CountAsync(a => a.CreatedAt >= today, ct);
        var weekCount = await _db.ActivityLogs.CountAsync(a => a.CreatedAt >= weekAgo, ct);
        var monthCount = await _db.ActivityLogs.CountAsync(a => a.CreatedAt >= monthAgo, ct);

        var byType = await _db.ActivityLogs.AsNoTracking()
            .Where(a => a.CreatedAt >= monthAgo)
            .GroupBy(a => a.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        var topUsers = await _db.ActivityLogs.AsNoTracking()
            .Where(a => a.CreatedAt >= monthAgo && a.UserId != null)
            .GroupBy(a => new { a.Username })
            .Select(g => new { g.Key.Username, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct);

        return Ok(new { total, today = todayCount, week = weekCount, month = monthCount, byType, topUsers });
    }

    // ===== POST: خروجی CSV =====
    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] ActivityLogFilterDto filter, CancellationToken ct)
    {
        if (!await HasPerm("logs.export", ct)) return Forbid();

        var q = _db.ActivityLogs.AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue) q = q.Where(a => a.UserId == filter.UserId);
        if (filter.FromDate.HasValue) q = q.Where(a => a.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) q = q.Where(a => a.CreatedAt <= filter.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.Type) &&
            Enum.TryParse<Storage.Domain.Entities.ActivityType>(filter.Type, true, out var type))
            q = q.Where(a => a.Type == type);

        var logs = await q.OrderByDescending(a => a.CreatedAt).Take(10000).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Time,Username,Type,Description,IP,ResourceType,ResourceId");
        foreach (var l in logs)
        {
            sb.AppendLine($"\"{l.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{l.Username}\",\"{l.Type}\",\"{l.Description?.Replace("\"", "'")}\",\"{l.IpAddress}\",\"{l.ResourceType}\",\"{l.ResourceId}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"activity-logs-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // ===== DELETE: پاک کردن (فقط admin.super) =====
    [HttpPost("clear")]
    public async Task<IActionResult> Clear([FromQuery] int daysOld = 30, CancellationToken ct = default)
    {
        if (!IsAdmin || !await HasPerm("logs.clear", ct)) return Forbid();

        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        var old = await _db.ActivityLogs.Where(a => a.CreatedAt < cutoff).ToListAsync(ct);
        _db.ActivityLogs.RemoveRange(old);
        await _db.SaveChangesAsync(ct);

        return Ok(new { removed = old.Count });
    }
}
