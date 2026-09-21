namespace Storage.Application.Dtos;

public sealed record GroupDto(
    Guid Id, string Name, string? Description,
    int MemberCount, int PermissionCount,
    bool IsSystemGroup, DateTime CreatedAt);

public sealed record GroupDetailDto(
    Guid Id, string Name, string? Description,
    bool IsSystemGroup, DateTime CreatedAt,
    List<GroupMemberDto> Members,
    List<PermissionDto> Permissions);

public sealed record GroupMemberDto(
    Guid Id, Guid UserId, string Username, DateTime JoinedAt);

public sealed record PermissionDto(
    Guid Id, string Code, string Name, string Category, string? Description);

public sealed record CreateGroupDto(string Name, string? Description);

public sealed record UpdateGroupDto(string Name, string? Description);

public sealed record AddMembersDto(List<Guid> UserIds);

public sealed record UpdateGroupPermissionsDto(List<Guid> PermissionIds);

public sealed record AddUserPermissionDto(Guid UserId, Guid PermissionId, DateTime? ExpiresAt);

public sealed record UserPermissionsDto(
    Guid UserId, List<PermissionDto> DirectPermissions,
    List<PermissionDto> GroupPermissions, List<PermissionDto> EffectivePermissions);

public sealed record GroupListDto(
    Guid Id, string Name, string? Description,
    int MemberCount, int PermissionCount, bool IsSystemGroup, DateTime CreatedAt);

public sealed record SetGroupPermissionsDto(List<Guid> PermissionIds);

public sealed record PermissionListDto(
    Guid Id, string Code, string Name, string Category, string? Description,
    int AssignedToGroups, int AssignedToUsers);

public sealed record AssignUserPermissionsDto(
    Guid UserId, List<Guid> PermissionIds, DateTime? ExpiresAt = null);
