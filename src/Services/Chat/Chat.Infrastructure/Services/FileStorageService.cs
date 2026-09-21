using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Chat.Infrastructure.Services;

public sealed class FileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<FileStorageService> _logger;

    private static readonly string[] AllowedImageExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedVideoExts = { ".mp4", ".webm", ".mov" };
    private static readonly string[] AllowedFileExts = { ".pdf", ".zip", ".docx", ".txt" };
    private const long MaxSizeBytes = 10 * 1024 * 1024;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads", "amazon-clone", "chat_uploads");

        Directory.CreateDirectory(Path.Combine(_rootPath, "images"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "videos"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "files"));
    }

    public (string Category, bool IsValid, string? Error) Validate(IFormFile file)
    {
        if (file.Length == 0) return ("files", false, "فایل خالی است");
        if (file.Length > MaxSizeBytes) return ("files", false, "حجم فایل بیش از 10MB است");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (AllowedImageExts.Contains(ext)) return ("images", true, null);
        if (AllowedVideoExts.Contains(ext)) return ("videos", true, null);
        if (AllowedFileExts.Contains(ext)) return ("files", true, null);

        return ("files", false, $"فرمت {ext} مجاز نیست");
    }

    public async Task<(string FileName, string FileUrl, long Size, string MimeType)> SaveAsync(
        IFormFile file, string category, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var folder = Path.Combine(_rootPath, category);
        var fullPath = Path.Combine(folder, uniqueName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var fileUrl = $"/uploads/{category}/{uniqueName}";
        return (file.FileName, fileUrl, file.Length, file.ContentType ?? "application/octet-stream");
    }
}
