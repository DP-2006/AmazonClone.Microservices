namespace Storage.Application.Dtos;

public sealed record UserSummaryDto(
    Guid Id, string Email, string FullName, string? PhoneNumber,
    bool IsActive, bool IsSeller, DateTime CreatedAt,
    List<string> Roles, List<GroupMembershipDto> Groups,
    int FileCount, long StorageUsedBytes, DateTime? LastActivityAt);

public sealed record GroupMembershipDto(Guid GroupId, string GroupName, DateTime JoinedAt);

public sealed record BlockUserDto(bool Block, string? Reason);

public sealed record ChangePasswordDto(string NewPassword);

public sealed record SendUserMessageDto(string Title, string Message, string Severity = "Info");

public sealed record UserActivitySummaryDto(
    Guid UserId, int LoginCount, int UploadCount,
    int DownloadCount, int DeleteCount, DateTime? LastActivityAt,
    List<ActivityLogDto> RecentActivities);

// ===== Bulk Operations =====
public sealed record BulkUserIdsDto(List<Guid> UserIds);

public sealed record BulkAssignRoleDto(Guid RoleId, List<Guid> UserIds);

public sealed record BulkPasswordDto(List<Guid> UserIds, string NewPassword);

public sealed record BulkBlockDto(List<Guid> UserIds, bool Block);

public sealed record BulkResultDto(int Success, int Failed, List<string> Errors);
