namespace Chat.Application.Chats.Dtos;

public sealed record CreateRoomDto(Guid SellerId);

public sealed record ChatRoomDto(
    Guid Id, Guid BuyerId, Guid SellerId,
    bool SellerChatEnabled, DateTime? LastMessageAt, DateTime CreatedAt,
    string? LastMessagePreview, int UnreadCount);

public sealed record SendMessageDto(string? Content, string Type = "Text");

public sealed record ChatMessageDto(
    Guid Id, Guid RoomId, Guid SenderId, string SenderName, string SenderRole,
    string? Content, string Type, bool IsDeleted, bool IsFlagged,
    DateTime CreatedAt, DateTime? ReadAt,
    List<ChatAttachmentDto> Attachments);

public sealed record ChatAttachmentDto(
    Guid Id, string FileName, string FileUrl, long FileSize, string MimeType);

public sealed record ReportMessageDto(string Reason);

public sealed record MessagePageDto(
    List<ChatMessageDto> Items, int Total, int Page, int PageSize);
