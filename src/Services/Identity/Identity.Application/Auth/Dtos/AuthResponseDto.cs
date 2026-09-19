namespace Identity.Application.Auth.Dtos;

public sealed record AuthResponseDto(string AccessToken, string Email, string FullName, DateTime ExpiresAt);
