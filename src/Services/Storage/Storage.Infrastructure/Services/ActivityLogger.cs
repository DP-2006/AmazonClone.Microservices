using Microsoft.AspNetCore.Http;
using Storage.Domain.Entities;
using Storage.Infrastructure.Data;

namespace Storage.Infrastructure.Services;

public sealed class ActivityLogger
{
    private readonly StorageDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ActivityLogger(StorageDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(Guid? userId, string username, ActivityType type,
        string description, string? resourceType = null, Guid? resourceId = null,
        string? metadata = null, CancellationToken ct = default)
    {
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var ua = _http.HttpContext?.Request.Headers["User-Agent"].ToString();

        var log = new ActivityLog(userId, username, type, description,
            resourceType, resourceId, ip, ua, metadata);

        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }
}
