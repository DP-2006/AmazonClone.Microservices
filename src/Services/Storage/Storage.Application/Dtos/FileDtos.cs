namespace Storage.Application.Dtos;

public sealed record StoredFileDto(
    Guid Id, Guid OwnerId, string OwnerName, Guid? FolderId,
    string FileName, string OriginalName, string FileUrl, long FileSize,
    string MimeType, string Extension, string? Description,
    bool IsPublic, int DownloadCount, DateTime CreatedAt);

public sealed record UpdateFileDto(string? Description, bool? IsPublic, Guid? FolderId);

public sealed record ShareFileDto(
    List<Guid> UserIds, string? Message = null,
    bool CanDownload = true, bool CanReshare = false, DateTime? ExpiresAt = null);

public sealed record FileShareDto(
    Guid Id, Guid FileId, string FileName, Guid SharedWithUserId,
    string? Message, bool CanDownload, bool CanReshare,
    DateTime? ExpiresAt, DateTime CreatedAt);
