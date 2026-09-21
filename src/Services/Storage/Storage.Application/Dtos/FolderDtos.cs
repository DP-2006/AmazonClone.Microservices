namespace Storage.Application.Dtos;

public sealed record FolderDto(
    Guid Id, Guid OwnerId, Guid? ParentFolderId, string Name,
    int FileCount, int SubFolderCount, DateTime CreatedAt);

public sealed record CreateFolderDto(string Name, Guid? ParentFolderId);

public sealed record RenameFolderDto(string Name);

public sealed record MoveFileDto(Guid? FolderId);
