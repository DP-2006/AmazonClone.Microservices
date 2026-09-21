namespace Storage.Domain.Entities;

public sealed class StorageSettings
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int MaxUploadSizeMB { get; private set; } = 100;
    public int MaxDownloadSizeMB { get; private set; } = 200;
    public int WarningThresholdMB { get; private set; } = 50;
    public long MaxUserStorageMB { get; private set; } = 5000;
    public bool AllowLargeFiles { get; private set; } = true;
    public string[] AllowedExtensions { get; private set; } = Array.Empty<string>();
    public string[] BlockedExtensions { get; private set; } = new[] { ".exe", ".bat", ".sh", ".ps1", ".jar" };
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? UpdatedByUserId { get; private set; }

    private StorageSettings() { }

    public static StorageSettings CreateDefault() => new();

    public void Update(int maxUpload, int maxDownload, int warning, long maxUserStorage,
        bool allowLarge, string[]? allowed, string[]? blocked, Guid? updatedBy)
    {
        MaxUploadSizeMB = maxUpload;
        MaxDownloadSizeMB = maxDownload;
        WarningThresholdMB = warning;
        MaxUserStorageMB = maxUserStorage;
        AllowLargeFiles = allowLarge;
        AllowedExtensions = allowed ?? Array.Empty<string>();
        BlockedExtensions = blocked ?? new[] { ".exe", ".bat", ".sh", ".ps1", ".jar" };
        UpdatedByUserId = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExtensionAllowed(string ext)
    {
        ext = ext.ToLowerInvariant();
        if (BlockedExtensions.Contains(ext)) return false;
        if (AllowedExtensions.Length == 0) return true;
        return AllowedExtensions.Contains(ext);
    }
}
