namespace Identity.Application.Auth.Dtos;

/// <summary>
/// درخواست ارسال OTP
/// </summary>
public sealed record SendOtpRequestDto(string Email);

/// <summary>
/// پاسخ ارسال OTP
/// </summary>
public sealed record SendOtpResponseDto(
    string Message,
    DateTime ExpiresAt,
    int ResendAfterSeconds = 60);

/// <summary>
/// درخواست تایید OTP
/// </summary>
public sealed record VerifyOtpRequestDto(string Email, string Code);

/// <summary>
/// پاسخ تایید OTP (همون AuthResponseDto قبلی)
/// </summary>
public sealed record OtpLoginResponseDto(
    string AccessToken,
    string Email,
    string FullName,
    DateTime ExpiresAt);
