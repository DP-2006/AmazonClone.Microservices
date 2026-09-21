namespace Identity.Application.Profile.Dtos;

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? PostalCode,
    string? Address,
    string? City,
    string? State,
    string? Country,
    bool IsSeller,
    DateTime CreatedAt);

public sealed record UpdateProfileDto(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? PostalCode,
    string? Address,
    string? City,
    string? State,
    string? Country);

public sealed record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword);

public sealed record ChangeEmailDto(
    string NewEmail,
    string Password);

public sealed record BecomeSellerDto(
    string BusinessName,
    string BusinessType,  // "Product" یا "Scientific"
    string Description,
    string? Website);
