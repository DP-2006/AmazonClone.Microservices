using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Dtos;
using Storage.Domain.Entities;
using Storage.Infrastructure.Clients;
using Storage.Infrastructure.Data;
using Storage.Infrastructure.Services;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly IdentityClient _identity;
    private readonly ActivityLogger _logger;
    private readonly PermissionService _perm;

    public UsersController(StorageDbContext db, IdentityClient identity,
        ActivityLogger logger, PermissionService perm)
    {
        _db = db;
        _identity = identity;
        _logger = logger;
        _perm = perm;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => User.IsInRole("Admin");

    private async Task<bool> HasPerm(string code, CancellationToken ct)
    {
        if (IsAdmin) return true;
        return await _perm.HasPermissionAsync(UserId, code, ct);
    }

    // ===== LIST =====
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct)
    {
        if (!await HasPerm("users.view", ct)) return Forbid();

        var authHeader = Request.Headers["Authorization"].ToString();
        var users = await _identity.ListUsersAsync(authHeader, ct);
        if (users.Count == 0) return Ok(new List<UserSummaryDto>());

        var ids = users.Select(u => u.Id).ToList();

        var fileStats = await _db.StoredFiles.AsNoTracking()
            .Where(f => !f.IsDeleted && ids.Contains(f.OwnerId))
            .GroupBy(f => f.OwnerId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), TotalSize = g.Sum(f => f.FileSize) })
            .ToDictionaryAsync(x => x.UserId, ct);

        var groupData = await _db.GroupMembers.AsNoTracking()
            .Where(m => ids.Contains(m.UserId))
            .Join(_db.Groups.AsNoTracking(), m => m.GroupId, g => g.Id,
                (m, g) => new { m.UserId, m.GroupId, g.Name, m.JoinedAt })
            .ToListAsync(ct);

        var groupsByUser = groupData
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => new GroupMembershipDto(x.GroupId, x.Name, x.JoinedAt)).ToList());

        var activities = await _db.ActivityLogs.AsNoTracking()
            .Where(a => a.UserId != null && ids.Contains(a.UserId.Value))
            .GroupBy(a => a.UserId!.Value)
            .Select(g => new { UserId = g.Key, LastAt = g.Max(a => a.CreatedAt) })
            .ToDictionaryAsync(x => x.UserId, x => x.LastAt, ct);

        var result = new List<UserSummaryDto>();
        foreach (var u in users)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                if (!(u.Email.ToLower().Contains(s) || u.FullName.ToLower().Contains(s)))
                    continue;
            }

            fileStats.TryGetValue(u.Id, out var stat);
            groupsByUser.TryGetValue(u.Id, out var userGroups);
            activities.TryGetValue(u.Id, out var lastActivity);

            result.Add(new UserSummaryDto(
                u.Id, u.Email, u.FullName, u.PhoneNumber,
                u.IsActive, u.IsSeller, u.CreatedAt,
                u.Roles, userGroups ?? new(),
                stat?.Count ?? 0, stat?.TotalSize ?? 0,
                lastActivity == default ? null : lastActivity));
        }

        return Ok(result.OrderBy(u => u.FullName).ToList());
    }

    // ===== GET DETAILS =====
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("users.view", ct) && UserId != id) return Forbid();

        var authHeader = Request.Headers["Authorization"].ToString();
        var user = await _identity.GetUserAsync(id, authHeader, ct);
        if (user is null) return NotFound("کاربر یافت نشد");

        var files = await _db.StoredFiles.AsNoTracking()
            .Where(f => f.OwnerId == id && !f.IsDeleted)
            .ToListAsync(ct);

        var groups = await _db.GroupMembers.AsNoTracking()
            .Where(m => m.UserId == id)
            .Join(_db.Groups, m => m.GroupId, g => g.Id,
                (m, g) => new GroupMembershipDto(g.Id, g.Name, m.JoinedAt))
            .ToListAsync(ct);

        var directPerms = await _db.UserPermissions.AsNoTracking()
            .Where(up => up.UserId == id && (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .Join(_db.Permissions, up => up.PermissionId, p => p.Id,
                (up, p) => new PermissionDto(p.Id, p.Code, p.Name, p.Category, p.Description))
            .ToListAsync(ct);

        return Ok(new
        {
            user,
            groups,
            directPermissions = directPerms,
            filesCount = files.Count,
            storageUsedBytes = files.Sum(f => f.FileSize)
        });
    }

    // ===== BLOCK =====
    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("users.block", ct)) return Forbid();
        if (id == UserId) return BadRequest("نمی‌توانید خودتان را مسدود کنید");

        var authHeader = Request.Headers["Authorization"].ToString();
        var ok = await _identity.SetBlockedAsync(id, true, authHeader, ct);
        if (!ok) return BadRequest("خطا در مسدود کردن کاربر");

        await _logger.LogAsync(UserId, UserName, ActivityType.UserBlocked,
            $"مسدود کردن کاربر {id}",
            resourceType: "User", resourceId: id, ct: ct);

        return Ok(new { message = "کاربر مسدود شد" });
    }

    // ===== UNBLOCK =====
    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("users.unblock", ct)) return Forbid();

        var authHeader = Request.Headers["Authorization"].ToString();
        var ok = await _identity.SetBlockedAsync(id, false, authHeader, ct);
        if (!ok) return BadRequest("خطا در فعال کردن کاربر");

        await _logger.LogAsync(UserId, UserName, ActivityType.UserUnblocked,
            $"رفع مسدودی کاربر {id}",
            resourceType: "User", resourceId: id, ct: ct);

        return Ok(new { message = "کاربر فعال شد" });
    }

    // ===== MESSAGE =====
    [HttpPost("{id:guid}/message")]
    public async Task<IActionResult> SendMessage(Guid id, SendUserMessageDto dto, CancellationToken ct)
    {
        if (!await HasPerm("users.message", ct)) return Forbid();

        var severity = Enum.TryParse<NotificationSeverity>(dto.Severity, true, out var s)
            ? s : NotificationSeverity.Info;

        var notif = new Notification(id, dto.Title, dto.Message, severity, null, UserId);
        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "پیام ارسال شد", notificationId = notif.Id });
    }

    // ===== ACTIVITY SUMMARY =====
    [HttpGet("{id:guid}/activity-summary")]
    public async Task<IActionResult> ActivitySummary(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("users.view_activity", ct)) return Forbid();

        var logs = await _db.ActivityLogs.AsNoTracking()
            .Where(a => a.UserId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        var summary = new UserActivitySummaryDto(
            id,
            LoginCount: await _db.ActivityLogs.CountAsync(a => a.UserId == id && a.Type == ActivityType.Login, ct),
            UploadCount: await _db.ActivityLogs.CountAsync(a => a.UserId == id && a.Type == ActivityType.Upload, ct),
            DownloadCount: await _db.ActivityLogs.CountAsync(a => a.UserId == id && a.Type == ActivityType.Download, ct),
            DeleteCount: await _db.ActivityLogs.CountAsync(a => a.UserId == id && a.Type == ActivityType.Delete, ct),
            LastActivityAt: logs.FirstOrDefault()?.CreatedAt,
            RecentActivities: logs.Select(a => new ActivityLogDto(
                a.Id, a.UserId, a.Username, a.Type.ToString(), a.Description,
                a.ResourceType, a.ResourceId, a.IpAddress, a.UserAgent,
                a.Metadata, a.CreatedAt)).ToList());

        return Ok(summary);
    }

    // ===== BULK: BLOCK =====
    [HttpPost("bulk/block")]
    public async Task<IActionResult> BulkBlock(BulkBlockDto dto, CancellationToken ct)
    {
        if (!await HasPerm("users.block", ct)) return Forbid();

        var authHeader = Request.Headers["Authorization"].ToString();
        var success = 0;
        var errors = new List<string>();

        foreach (var uid in dto.UserIds.Distinct())
        {
            if (uid == UserId) { errors.Add($"{uid}: نمی‌توانید خودتان را تغییر دهید"); continue; }
            try
            {
                var ok = await _identity.SetBlockedAsync(uid, dto.Block, authHeader, ct);
                if (ok) success++;
                else errors.Add($"{uid}: ناموفق");
            }
            catch (Exception ex) { errors.Add($"{uid}: {ex.Message}"); }
        }

        await _logger.LogAsync(UserId, UserName,
            dto.Block ? ActivityType.UserBlocked : ActivityType.UserUnblocked,
            $"عملیات گروهی: {success} کاربر {(dto.Block ? "مسدود" : "فعال")}",
            resourceType: "User", ct: ct);

        return Ok(new BulkResultDto(success, errors.Count, errors));
    }

    // ===== BULK: ASSIGN GROUP =====
    [HttpPost("bulk/assign-group")]
    public async Task<IActionResult> BulkAssignGroup(BulkAssignRoleDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.manage_members", ct)) return Forbid();

        var group = await _db.Groups.Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == dto.RoleId, ct);
        if (group is null) return NotFound("گروه یافت نشد");

        var success = 0;
        var errors = new List<string>();

        foreach (var uid in dto.UserIds.Distinct())
        {
            try
            {
                if (group.Members.Any(m => m.UserId == uid)) { errors.Add($"{uid}: قبلاً عضو بود"); continue; }
                _db.GroupMembers.Add(new GroupMember(group.Id, uid, uid.ToString()[..8], UserId));
                success++;
            }
            catch (Exception ex) { errors.Add($"{uid}: {ex.Message}"); }
        }

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"افزودن گروهی {success} کاربر به گروه {group.Name}",
            resourceType: "Group", resourceId: group.Id, ct: ct);

        return Ok(new BulkResultDto(success, errors.Count, errors));
    }

    // ===== BULK: CHECK GROUP MEMBERS =====
    [HttpGet("bulk/check-group/{groupId:guid}")]
    public async Task<IActionResult> CheckGroupMembers(Guid groupId, CancellationToken ct)
    {
        if (!await HasPerm("groups.view", ct)) return Forbid();

        var count = await _db.GroupMembers.CountAsync(m => m.GroupId == groupId, ct);
        return Ok(new { groupId, memberCount = count });
    }
}
