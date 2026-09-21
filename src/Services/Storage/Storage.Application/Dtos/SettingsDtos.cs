namespace Storage.Application.Dtos;

public sealed record StorageSettingsDto(
    int MaxUploadSizeMB, int MaxDownloadSizeMB, int WarningThresholdMB,
    long MaxUserStorageMB, bool AllowLargeFiles,
    string[] AllowedExtensions, string[] BlockedExtensions,
    DateTime UpdatedAt);

public sealed record UpdateStorageSettingsDto(
    int MaxUploadSizeMB, int MaxDownloadSizeMB, int WarningThresholdMB,
    long MaxUserStorageMB, bool AllowLargeFiles,
    string[]? AllowedExtensions, string[]? BlockedExtensions);
