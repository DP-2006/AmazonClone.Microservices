namespace Identity.Application.Auth.Dtos;

public sealed record RegisterDto(string Email, string Password, string FirstName, string LastName);
