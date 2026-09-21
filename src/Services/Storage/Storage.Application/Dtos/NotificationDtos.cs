namespace Storage.Application.Dtos;

public sealed record NotificationDto(
    Guid Id, string Title, string Message, string Severity,
    bool IsRead, Guid? RelatedFileId, DateTime CreatedAt, DateTime? ReadAt);

public sealed record SendMessageDto(Guid UserId, string Title, string Message, string Severity = "Info");

public sealed record BroadcastMessageDto(
    List<Guid> UserIds, string Title, string Message, string Severity = "Info");
