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
[Route("api/storage/groups")]
[Authorize]
public sealed class GroupsController : ControllerBase
{
    private readonly StorageDbContext _db;
    private readonly PermissionService _perm;
    private readonly ActivityLogger _logger;

    public GroupsController(StorageDbContext db, PermissionService perm, ActivityLogger logger)
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

    // ===== LIST =====
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var groups = await _db.Groups.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GroupListDto(
                g.Id, g.Name, g.Description,
                g.Members.Count, g.Permissions.Count,
                g.IsSystemGroup, g.CreatedAt))
            .ToListAsync(ct);

        return Ok(groups);
    }

    // ===== GET =====
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var group = await _db.Groups.AsNoTracking()
            .Include(g => g.Members)
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null) return NotFound();

        var permIds = group.Permissions.Select(p => p.PermissionId).ToList();
        var perms = await _db.Permissions.AsNoTracking()
            .Where(p => permIds.Contains(p.Id))
            .Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Category, p.Description))
            .ToListAsync(ct);

        return Ok(new GroupDetailDto(
            group.Id, group.Name, group.Description, group.IsSystemGroup, group.CreatedAt,
            group.Members.Select(m => new GroupMemberDto(m.Id, m.UserId, m.Username, m.JoinedAt)).ToList(),
            perms));
    }

    // ===== CREATE =====
    [HttpPost]
    public async Task<IActionResult> Create(CreateGroupDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.create", ct)) return Forbid();

        var exists = await _db.Groups.AnyAsync(g => g.Name == dto.Name.Trim(), ct);
        if (exists) return Conflict($"گروه «{dto.Name}» قبلاً وجود دارد");

        var group = new Group(dto.Name, UserId, dto.Description);
        _db.Groups.Add(group);
        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"ساخت گروه جدید: {dto.Name}",
            resourceType: "Group", resourceId: group.Id, ct: ct);

        return Ok(new GroupListDto(group.Id, group.Name, group.Description, 0, 0, false, group.CreatedAt));
    }

    // ===== UPDATE =====
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateGroupDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.edit", ct)) return Forbid();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();
        if (group.IsSystemGroup) return BadRequest("گروه‌های سیستمی قابل ویرایش نیستند");

        var nameExists = await _db.Groups.AnyAsync(g => g.Id != id && g.Name == dto.Name.Trim(), ct);
        if (nameExists) return Conflict($"گروه «{dto.Name}» قبلاً وجود دارد");

        group.Rename(dto.Name);
        group.UpdateDescription(dto.Description);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ===== DELETE =====
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!await HasPerm("groups.delete", ct)) return Forbid();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();
        if (group.IsSystemGroup) return BadRequest("گروه‌های سیستمی قابل حذف نیستند");

        var name = group.Name;
        _db.Groups.Remove(group);
        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"حذف گروه: {name}",
            resourceType: "Group", resourceId: id, ct: ct);

        return NoContent();
    }

    // ===== ADD MEMBERS =====
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMembers(Guid id, AddMembersDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.manage_members", ct)) return Forbid();

        var group = await _db.Groups.Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();

        var added = 0;
        foreach (var uid in dto.UserIds.Distinct())
        {
            if (group.Members.Any(m => m.UserId == uid)) continue;
            _db.GroupMembers.Add(new GroupMember(id, uid, uid.ToString()[..8], UserId));
            added++;
        }

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"افزودن {added} عضو به گروه {group.Name}",
            resourceType: "Group", resourceId: id, ct: ct);

        return Ok(new { added });
    }

    // ===== REMOVE MEMBER =====
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        if (!await HasPerm("groups.manage_members", ct)) return Forbid();

        var member = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId, ct);
        if (member is null) return NotFound();

        _db.GroupMembers.Remove(member);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ===== SET PERMISSIONS =====
    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> SetPermissions(Guid id, SetGroupPermissionsDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.manage_permissions", ct)) return Forbid();

        var group = await _db.Groups.Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();

        _db.GroupPermissions.RemoveRange(group.Permissions);

        foreach (var permId in dto.PermissionIds.Distinct())
            _db.GroupPermissions.Add(new GroupPermission(id, permId, UserId));

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"تنظیم {dto.PermissionIds.Count} دسترسی برای گروه {group.Name}",
            resourceType: "Group", resourceId: id, ct: ct);

        return Ok(new { count = dto.PermissionIds.Count });
    }

    // ===== GET PASSWORD POLICY =====
    [HttpGet("{id:guid}/password-policy")]
    public async Task<IActionResult> GetPasswordPolicy(Guid id, CancellationToken ct)
    {
        if (!IsAdmin) return Forbid();

        var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();

        return Ok(new PasswordPolicyDto(
            group.MinPasswordLength, group.RequireUppercase, group.RequireDigit,
            group.RequireLowercase, group.RequireSpecialChar,
            group.PasswordExpiryDays, group.MaxLoginAttempts));
    }

    // ===== SET PASSWORD POLICY =====
    [HttpPut("{id:guid}/password-policy")]
    public async Task<IActionResult> SetPasswordPolicy(Guid id, UpdatePasswordPolicyDto dto, CancellationToken ct)
    {
        if (!await HasPerm("groups.edit", ct)) return Forbid();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return NotFound();

        group.SetPasswordPolicy(
            dto.MinPasswordLength, dto.RequireUppercase, dto.RequireDigit,
            dto.RequireLowercase, dto.RequireSpecialChar,
            dto.PasswordExpiryDays, dto.MaxLoginAttempts);

        await _db.SaveChangesAsync(ct);

        await _logger.LogAsync(UserId, UserName, ActivityType.PermissionChange,
            $"تغییر Password Policy گروه {group.Name}",
            resourceType: "Group", resourceId: id, ct: ct);

        return Ok(new PasswordPolicyDto(
            group.MinPasswordLength, group.RequireUppercase, group.RequireDigit,
            group.RequireLowercase, group.RequireSpecialChar,
            group.PasswordExpiryDays, group.MaxLoginAttempts));
    }
}
