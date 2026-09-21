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
[Route("api/storage/permissions")]
[Authorize]
public sealed class PermissionsController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly PermissionService _perm;
    private readonly ActivityLogger _logger;

    public PermissionsController(StorageDbContext db, PermissionService perm, ActivityLogger logger)
    {
        _db = db;
        _perm = perm;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserName => User.FindFirstValue(ClaimTypes.Name) ?? "کاربر";
    private bool IsAdmin => User.IsInRole("Admin");

    // ========== GET: لیست همه Permission ها ==========
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var perms = await _db.Permissions.AsNoTracking()
            .OrderBy(p => p.Category).ThenBy(p => p.Code)
            .Select(p => new PermissionListDto(
                p.Id, p.Code, p.Name, p.Category, p.Description,
                _db.GroupPermissions.Count(gp => gp.PermissionId == p.Id),
                _db.UserPermissions.Count(up => up.PermissionId == p.Id)))
            .ToListAsync(ct);

        return Ok(perms);
    }

    // ========== GET: گروه‌بندی بر اساس Category ==========
    [HttpGet("by-category")]
    public async Task<IActionResult> ListByCategory(CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var perms = await _db.Permissions.AsNoTracking()
            .OrderBy(p => p.Category).ThenBy(p => p.Code)
            .ToListAsync(ct);

        var grouped = perms
            .GroupBy(p => p.Category)
            .Select(g => new
            {
                category = g.Key,
                permissions = g.Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Category, p.Description)).ToList()
            })
            .ToList();

        return Ok(grouped);
    }

    // ========== GET: Permission های یک کاربر ==========
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken ct)
    {
        if (!IsAdmin && userId != UserId) return Forbid();

        // مستقیم
        var direct = await _db.UserPermissions.AsNoTracking()
            .Where(up => up.UserId == userId && (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .Join(_db.Permissions, up => up.PermissionId, p => p.Id,
                (up, p) => new PermissionDto(p.Id, p.Code, p.Name, p.Category, p.Description))
            .ToListAsync(ct);

        // از گروه‌ها
        var viaGroups = await (from gm in _db.GroupMembers.AsNoTracking()
                               join gp in _db.GroupPermissions.AsNoTracking() on gm.GroupId equals gp.GroupId
                               join p in _db.Permissions.AsNoTracking() on gp.PermissionId equals p.Id
                               where gm.UserId == userId
                               select new PermissionDto(p.Id, p.Code, p.Name, p.Category, p.Description))
                               .Distinct()
                               .ToListAsync(ct);

        // مؤثر = union
        var effective = direct.Concat(viaGroups)
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();

        return Ok(new UserPermissionsDto(userId, direct, viaGroups, effective));
    }

    // ========== POST: اختصاص مستقیم به کاربر ==========
    [HttpPost("assign")]
    public async Task<IActionResult> AssignToUser(AssignUserPermissionsDto dto, CancellationToken ct)
    {
        if (!await _perm.HasPermissionAsync(UserId, "permissions.assign", ct)) return Forbid();

        var added = 0;
        foreach (var permId in dto.PermissionIds.Distinct())
        {
            var exists = await _db.UserPermissions
                .AnyAsync(up => up.UserId == dto.UserId && up.PermissionId == permId, ct);
            if (exists) continue;

            _db.UserPermissions.Add(new UserPermission(dto.UserId, permId, UserId, dto.ExpiresAt));
            added++;
        }

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"اختصاص {added} دسترسی مستقیم به کاربر {dto.UserId}",
            resourceType: "User", resourceId: dto.UserId, ct: ct);

        return Ok(new { added });
    }

    // ========== DELETE: حذف دسترسی مستقیم ==========
    [HttpDelete("user/{userId:guid}/permission/{permissionId:guid}")]
    public async Task<IActionResult> RevokeFromUser(Guid userId, Guid permissionId, CancellationToken ct)
    {
        if (!await _perm.HasPermissionAsync(UserId, "permissions.assign", ct)) return Forbid();

        var up = await _db.UserPermissions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PermissionId == permissionId, ct);
        if (up is null) return NotFound();

        _db.UserPermissions.Remove(up);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ========== GET: Permission های خودم ==========
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPermissions(CancellationToken ct)
    {
        var perms = await _perm.GetEffectivePermissionsAsync(UserId, ct);
        return Ok(new
        {
            userId = UserId,
            isAdmin = IsAdmin,
            permissions = perms
        });
    }
}
