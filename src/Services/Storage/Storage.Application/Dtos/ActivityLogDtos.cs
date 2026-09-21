namespace Storage.Application.Dtos;

public sealed record ActivityLogDto(
    Guid Id, Guid? UserId, string Username, string Type, string Description,
    string? ResourceType, Guid? ResourceId, string? IpAddress,
    string? UserAgent, string? Metadata, DateTime CreatedAt);

public sealed record ActivityLogFilterDto(
    Guid? UserId,
    string? Username,
    string? Type,
    DateTime? FromDate,
    DateTime? ToDate,
    string? ResourceType,
    string? Search,
    int Page = 1,
    int PageSize = 50);

public sealed record PagedResultDto<T>(
    List<T> Items, int Total, int Page, int PageSize, int TotalPages);
