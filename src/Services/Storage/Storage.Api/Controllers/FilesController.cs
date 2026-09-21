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
[Route("api/storage/files")]
[Authorize]
public sealed class FilesController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly FileStorageService _storage;
    private readonly ActivityLogger _logger;
    private readonly PermissionService _perm;

    public FilesController(StorageDbContext db, FileStorageService storage,
        ActivityLogger logger, PermissionService perm)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
        _perm = perm;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet("my")]
    public async Task<IActionResult> GetMyFiles([FromQuery] Guid? folderId, CancellationToken ct)
    {
        var q = _db.StoredFiles.AsNoTracking()
            .Where(f => f.OwnerId == UserId && !f.IsDeleted);

        q = folderId.HasValue ? q.Where(f => f.FolderId == folderId) : q.Where(f => f.FolderId == null);

        var files = await q.OrderByDescending(f => f.CreatedAt)
            .Select(f => new StoredFileDto(f.Id, f.OwnerId, f.OwnerName, f.FolderId,
                f.FileName, f.OriginalName, f.FileUrl, f.FileSize,
                f.MimeType, f.Extension, f.Description, f.IsPublic,
                f.DownloadCount, f.CreatedAt))
            .ToListAsync(ct);

        return Ok(files);
    }

    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedWithMe(CancellationToken ct)
    {
        var rows = await _db.FileShares.AsNoTracking()
            .Where(s => s.SharedWithUserId == UserId
                && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .Join(_db.StoredFiles.AsNoTracking().Where(f => !f.IsDeleted),
                s => s.FileId, f => f.Id,
                (s, f) => new
                {
                    s.Id, s.FileId, f.FileName, s.SharedWithUserId,
                    s.Message, s.CanDownload, s.CanReshare, s.ExpiresAt, s.CreatedAt
                })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var shares = rows.Select(s => new FileShareDto(
            s.Id, s.FileId, s.FileName, s.SharedWithUserId,
            s.Message, s.CanDownload, s.CanReshare, s.ExpiresAt, s.CreatedAt))
            .ToList();

        return Ok(shares);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(105 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file,
        [FromForm] Guid? folderId, [FromForm] string? description, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("فایل ارسال نشده");

        var settings = await _db.StorageSettings.FirstOrDefaultAsync(ct) ?? StorageSettings.CreateDefault();

        if (file.Length > settings.MaxUploadSizeMB * 1024L * 1024L)
            return BadRequest($"حجم فایل بیش از {settings.MaxUploadSizeMB}MB است");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!settings.IsExtensionAllowed(ext))
            return BadRequest($"پسوند {ext} مجاز نیست");

        var (storedName, url, size, mime, extension) = await _storage.SaveAsync(file, UserId, ct);

        var storedFile = new StoredFile(UserId, UserName, storedName, file.FileName,
            url, size, mime, extension, folderId);

        if (!string.IsNullOrWhiteSpace(description))
            storedFile.UpdateDescription(description);

        _db.StoredFiles.Add(storedFile);
        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Upload,
            $"آپلود فایل: {file.FileName}",
            resourceType: "File", resourceId: storedFile.Id, ct: ct);

        return Ok(new StoredFileDto(storedFile.Id, storedFile.OwnerId, storedFile.OwnerName,
            storedFile.FolderId, storedFile.FileName, storedFile.OriginalName,
            storedFile.FileUrl, storedFile.FileSize, storedFile.MimeType,
            storedFile.Extension, storedFile.Description, storedFile.IsPublic,
            storedFile.DownloadCount, storedFile.CreatedAt));
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await _db.StoredFiles.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);
        if (file is null) return NotFound();

        var hasAccess = file.OwnerId == UserId || file.IsPublic || IsAdmin;

        if (!hasAccess)
        {
            var share = await _db.FileShares
                .FirstOrDefaultAsync(s => s.FileId == id && s.SharedWithUserId == UserId
                    && s.CanDownload
                    && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow), ct);
            hasAccess = share is not null;
        }

        if (!hasAccess) return Forbid();

        var physicalPath = _storage.GetPhysicalPath(file.FileUrl);
        if (!System.IO.File.Exists(physicalPath)) return NotFound("فایل فیزیکی یافت نشد");

        file.IncrementDownload();
        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Download,
            $"دانلود فایل: {file.OriginalName}",
            resourceType: "File", resourceId: file.Id, ct: ct);

        return PhysicalFile(physicalPath, file.MimeType, file.OriginalName);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFileDto dto, CancellationToken ct)
    {
        var file = await _db.StoredFiles.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);
        if (file is null) return NotFound();
        if (file.OwnerId != UserId && !IsAdmin) return Forbid();

        if (dto.Description is not null) file.UpdateDescription(dto.Description);
        if (dto.IsPublic.HasValue) file.SetPublic(dto.IsPublic.Value);
        if (dto.FolderId.HasValue) file.SetFolder(dto.FolderId);

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Edit,
            $"ویرایش فایل: {file.OriginalName}",
            resourceType: "File", resourceId: file.Id, ct: ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var file = await _db.StoredFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null) return NotFound();
        if (file.OwnerId != UserId && !IsAdmin) return Forbid();

        file.SoftDelete();
        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Delete,
            $"حذف فایل: {file.OriginalName}",
            resourceType: "File", resourceId: file.Id, ct: ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/share")]
    public async Task<IActionResult> Share(Guid id, ShareFileDto dto, CancellationToken ct)
    {
        var file = await _db.StoredFiles.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct);
        if (file is null) return NotFound();
        if (file.OwnerId != UserId && !IsAdmin) return Forbid();

        var created = new List<Guid>();
        foreach (var uid in dto.UserIds.Distinct())
        {
            if (uid == UserId) continue;
            var exists = await _db.FileShares.AnyAsync(s => s.FileId == id && s.SharedWithUserId == uid, ct);
            if (exists) continue;

            _db.FileShares.Add(new Storage.Domain.Entities.FileShare(id, UserId, uid, dto.Message,
                dto.CanDownload, dto.CanReshare, dto.ExpiresAt));
            created.Add(uid);
        }

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.Share,
            $"اشتراک فایل {file.OriginalName} با {created.Count} کاربر",
            resourceType: "File", resourceId: file.Id, ct: ct);

        return Ok(new { sharedWith = created.Count });
    }

    [HttpDelete("{id:guid}/share/{userId:guid}")]
    public async Task<IActionResult> Unshare(Guid id, Guid userId, CancellationToken ct)
    {
        var file = await _db.StoredFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null) return NotFound();
        if (file.OwnerId != UserId && !IsAdmin) return Forbid();

        var share = await _db.FileShares.FirstOrDefaultAsync(s => s.FileId == id && s.SharedWithUserId == userId, ct);
        if (share is null) return NotFound();

        _db.FileShares.Remove(share);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
