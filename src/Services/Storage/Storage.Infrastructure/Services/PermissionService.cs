using Microsoft.EntityFrameworkCore;
using Storage.Infrastructure.Data;

namespace Storage.Infrastructure.Services;

public sealed class PermissionService
{
    private readonly StorageDbContext _db;
    public PermissionService(StorageDbContext db) => _db = db;

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        var perm = await _db.Permissions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == permissionCode, ct);
        if (perm is null) return false;

        var direct = await _db.UserPermissions.AsNoTracking()
            .AnyAsync(up => up.UserId == userId && up.PermissionId == perm.Id
                && (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow), ct);
        if (direct) return true;

        return await (from gm in _db.GroupMembers.AsNoTracking()
                      join gp in _db.GroupPermissions.AsNoTracking() on gm.GroupId equals gp.GroupId
                      where gm.UserId == userId && gp.PermissionId == perm.Id
                      select gp.Id).AnyAsync(ct);
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var direct = await _db.UserPermissions.AsNoTracking()
            .Where(up => up.UserId == userId && (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .Join(_db.Permissions, up => up.PermissionId, p => p.Id, (up, p) => p.Code)
            .ToListAsync(ct);

        var viaGroups = await (from gm in _db.GroupMembers.AsNoTracking()
                               join gp in _db.GroupPermissions.AsNoTracking() on gm.GroupId equals gp.GroupId
                               join p in _db.Permissions.AsNoTracking() on gp.PermissionId equals p.Id
                               where gm.UserId == userId
                               select p.Code).ToListAsync(ct);

        return direct.Concat(viaGroups).Distinct().OrderBy(x => x).ToList();
    }

    public async Task SeedDefaultPermissionsAsync(CancellationToken ct = default)
    {
        if (await _db.Permissions.AnyAsync(ct)) return;

        var perms = new[]
        {
            ("files.upload", "آپلود فایل", "Files"),
            ("files.download", "دانلود فایل", "Files"),
            ("files.delete", "حذف فایل خود", "Files"),
            ("files.delete.any", "حذف فایل هر کاربر", "Files"),
            ("files.view.any", "مشاهده فایل هر کاربر", "Files"),
            ("files.share", "اشتراک فایل", "Files"),
            ("folders.create", "ساخت پوشه", "Folders"),
            ("folders.delete", "حذف پوشه", "Folders"),
            ("users.view", "مشاهده کاربران", "Users"),
            ("users.block", "مسدود کردن کاربر", "Users"),
            ("users.unblock", "رفع مسدودی کاربر", "Users"),
            ("users.delete", "حذف کاربر", "Users"),
            ("users.change_password", "تغییر رمز کاربران", "Users"),
            ("users.message", "پیام دادن به کاربران", "Users"),
            ("users.view_activity", "مشاهده فعالیت کاربران", "Users"),
            ("groups.create", "ساخت گروه", "Groups"),
            ("groups.edit", "ویرایش گروه", "Groups"),
            ("groups.delete", "حذف گروه", "Groups"),
            ("groups.manage_members", "مدیریت اعضای گروه", "Groups"),
            ("groups.manage_permissions", "مدیریت دسترسی‌های گروه", "Groups"),
            ("groups.view", "مشاهده گروه‌ها", "Groups"),
            ("permissions.assign", "اختصاص دسترسی مستقیم", "Permissions"),
            ("permissions.view", "مشاهده دسترسی‌ها", "Permissions"),
            ("logs.view", "مشاهده لاگ‌ها", "Logs"),
            ("logs.clear", "پاک کردن لاگ‌ها", "Logs"),
            ("logs.export", "خروجی لاگ‌ها", "Logs"),
            ("settings.view", "مشاهده تنظیمات", "Settings"),
            ("settings.edit", "ویرایش تنظیمات", "Settings"),
            ("admin.panel", "دسترسی به پنل ادمین", "Admin"),
            ("admin.super", "دسترسی سوپر ادمین", "Admin")
        };

        foreach (var (code, name, category) in perms)
            _db.Permissions.Add(new Domain.Entities.Permission(code, name, category));

        await _db.SaveChangesAsync(ct);
    }
}
