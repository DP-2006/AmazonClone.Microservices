using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Dtos;
using Storage.Domain.Entities;
using Storage.Infrastructure.Data;
using Storage.Infrastructure.Services;

namespace Storage.Api.Controllers;

[ApiController]
[Route("api/storage/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly PermissionService _perm;
    private readonly ActivityLogger _logger;

    public SettingsController(StorageDbContext db, PermissionService perm, ActivityLogger logger)
    {
        _db = db;
        _perm = perm;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => User.IsInRole("Admin");

    private async Task<bool> HasPerm(string code, CancellationToken ct)
    {
        if (IsAdmin) return true;
        return await _perm.HasPermissionAsync(UserId, code, ct);
    }

    // ===== GET: تنظیمات فعلی =====
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var settings = await _db.StorageSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = StorageSettings.CreateDefault();
            _db.StorageSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new StorageSettingsDto(
            settings.MaxUploadSizeMB, settings.MaxDownloadSizeMB,
            settings.WarningThresholdMB, settings.MaxUserStorageMB,
            settings.AllowLargeFiles, settings.AllowedExtensions,
            settings.BlockedExtensions, settings.UpdatedAt));
    }

    // ===== PUT: ویرایش (فقط admin.super) =====
    [HttpPut]
    public async Task<IActionResult> Update(UpdateStorageSettingsDto dto, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();
        if (!await HasPerm("admin.super", ct)) return Forbid();

        if (dto.MaxUploadSizeMB < 1 || dto.MaxUploadSizeMB > 10240)
            return BadRequest("حجم آپلود باید بین ۱ تا ۱۰۲۴۰ مگابایت باشد");
        if (dto.MaxDownloadSizeMB < 1 || dto.MaxDownloadSizeMB > 10240)
            return BadRequest("حجم دانلود باید بین ۱ تا ۱۰۲۴۰ مگابایت باشد");
        if (dto.WarningThresholdMB < 1 || dto.WarningThresholdMB > dto.MaxUploadSizeMB)
            return BadRequest("آستانه هشدار باید بین ۱ و حجم آپلود باشد");

        var settings = await _db.StorageSettings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = StorageSettings.CreateDefault();
            _db.StorageSettings.Add(settings);
        }

        settings.Update(
            dto.MaxUploadSizeMB, dto.MaxDownloadSizeMB, dto.WarningThresholdMB,
            dto.MaxUserStorageMB, dto.AllowLargeFiles,
            dto.AllowedExtensions, dto.BlockedExtensions, UserId);

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Edit,
            "به‌روزرسانی تنظیمات Storage",
            resourceType: "Settings", ct: ct);

        return Ok(new StorageSettingsDto(
            settings.MaxUploadSizeMB, settings.MaxDownloadSizeMB,
            settings.WarningThresholdMB, settings.MaxUserStorageMB,
            settings.AllowLargeFiles, settings.AllowedExtensions,
            settings.BlockedExtensions, settings.UpdatedAt));
    }

    // ===== GET: آمار کلی سیستم =====
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        if (!await HasPerm("settings.view", ct)) return Forbid();

        var totalFiles = await _db.StoredFiles.CountAsync(f => !f.IsDeleted, ct);
        var totalFolders = await _db.Folders.CountAsync(f => !f.IsDeleted, ct);
        var totalSize = await _db.StoredFiles
            .Where(f => !f.IsDeleted)
            .SumAsync(f => (long?)f.FileSize, ct) ?? 0;
        var totalShares = await _db.FileShares.CountAsync(ct);
        var totalGroups = await _db.Groups.CountAsync(ct);
        var totalPermissions = await _db.Permissions.CountAsync(ct);

        // کاربران منحصر‌به‌فرد که فایل دارن
        var activeUsers = await _db.StoredFiles
            .Where(f => !f.IsDeleted)
            .Select(f => f.OwnerId)
            .Distinct()
            .CountAsync(ct);

        // لاگ‌ها
        var totalLogs = await _db.ActivityLogs.CountAsync(ct);
        var logsToday = await _db.ActivityLogs
            .CountAsync(a => a.CreatedAt >= DateTime.UtcNow.Date, ct);

        // توزیع حجم بر اساس پسوند
        var extDistribution = await _db.StoredFiles.AsNoTracking()
            .Where(f => !f.IsDeleted)
            .GroupBy(f => f.Extension)
            .Select(g => new { Extension = g.Key, Count = g.Count(), Size = g.Sum(f => f.FileSize) })
            .OrderByDescending(x => x.Size)
            .Take(15)
            .ToListAsync(ct);

        return Ok(new
        {
            files = new { total = totalFiles, sizeBytes = totalSize, sizeMB = Math.Round(totalSize / 1024.0 / 1024.0, 2) },
            folders = totalFolders,
            shares = totalShares,
            groups = totalGroups,
            permissions = totalPermissions,
            activeUsers,
            logs = new { total = totalLogs, today = logsToday },
            byExtension = extDistribution.Select(x => new
            {
                extension = x.Extension,
                count = x.Count,
                sizeMB = Math.Round(x.Size / 1024.0 / 1024.0, 2)
            })
        });
    }

    // ===== GET: پاکسازی دیتابیس (فقط سوپر ادمین) =====
    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup([FromQuery] int daysOld = 30, CancellationToken ct = default)
    {
        if (!IsAdmin || !await HasPerm("admin.super", ct)) return Forbid();

        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        // حذف لاگ‌های قدیمی
        var oldLogs = await _db.ActivityLogs.Where(a => a.CreatedAt < cutoff).ToListAsync(ct);
        _db.ActivityLogs.RemoveRange(oldLogs);

        // حذف notification های خوانده‌شده قدیمی
        var oldNotifs = await _db.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ToListAsync(ct);
        _db.Notifications.RemoveRange(oldNotifs);

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Edit,
            $"پاکسازی: {oldLogs.Count} لاگ قدیمی، {oldNotifs.Count} نوتیفیکیشن قدیمی",
            resourceType: "Settings", ct: ct);

        return Ok(new
        {
            removedLogs = oldLogs.Count,
            removedNotifications = oldNotifs.Count
        });
    }
}
