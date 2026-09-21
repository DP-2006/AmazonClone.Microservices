using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Storage.Infrastructure.Services;

public sealed class FileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads", "amazon-clone", "storage_uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string StoredName, string Url, long Size, string Mime, string Ext)>
        SaveAsync(IFormFile file, Guid ownerId, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var ownerFolder = Path.Combine(_rootPath, ownerId.ToString("N"));
        Directory.CreateDirectory(ownerFolder);
        var fullPath = Path.Combine(ownerFolder, storedName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream, ct);
        var url = $"/storage/{ownerId:N}/{storedName}";
        var mime = file.ContentType ?? "application/octet-stream";
        return (storedName, url, file.Length, mime, ext);
    }

    public bool Delete(string storedPath)
    {
        try { if (File.Exists(storedPath)) { File.Delete(storedPath); return true; } }
        catch (Exception ex) { _logger.LogError(ex, "Error deleting {Path}", storedPath); }
        return false;
    }

    public string GetPhysicalPath(string fileUrl)
    {
        var relative = fileUrl.TrimStart('/').Replace("storage/", "");
        return Path.Combine(_rootPath, relative);
    }
}
