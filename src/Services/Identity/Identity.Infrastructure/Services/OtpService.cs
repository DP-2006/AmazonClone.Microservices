using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Messaging.Contracts;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Services;

/// <summary>
/// سرویس تولید و تایید OTP
/// </summary>
public sealed class OtpService
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;

    public OtpService(
        IdentityDbContext db,
        IPublishEndpoint publishEndpoint,
        ILogger<OtpService> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// تولید کد ۶ رقمی امن
    /// </summary>
    private static string GenerateSecureOtp()
    {
        // استفاده از RandomNumberGenerator به‌جای Random
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return number.ToString("D6");
    }

    /// <summary>
    /// Hash کردن کد با SHA256 (ساده ولی امن)
    /// </summary>
    private static string HashCode(string code, string email)
    {
        // Salt از email + یک secret ثابت
        var salted = $"{email}:{code}:OTP_SALT_CHANGE_ME_IN_PRODUCTION";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// ارسال OTP جدید
    /// </summary>
    public async Task<(bool Success, string Message, DateTime? ExpiresAt)> SendOtpAsync(
        string email,
        CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();

        // ۱. بررسی وجود کاربر
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);

        if (user is null)
        {
            // به دلایل امنیتی، پیام یکسان برمی‌گردونیم
            // تا مهاجم نفهمه ایمیل وجود داره یا نه
            _logger.LogWarning("OTP request for non-existent email: {Email}", email);
            return (true, "اگر ایمیل شما در سیستم باشد، کد ارسال خواهد شد.", null);
        }

        // ۲. بررسی cooldown (جلوگیری از spam)
        var lastOtp = await _db.Set<OtpCode>()
            .Where(o => o.UserId == user.Id && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (lastOtp is not null &&
            (DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds < ResendCooldownSeconds)
        {
            var wait = ResendCooldownSeconds - (int)(DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds;
            return (false, $"لطفاً {wait} ثانیه دیگر تلاش کنید.", null);
        }

        // ۳. غیرفعال کردن کدهای قبلی
        if (lastOtp is not null)
        {
            lastOtp.MarkUsed();
        }

        // ۴. تولید کد جدید
        var code = GenerateSecureOtp();
        var codeHash = HashCode(code, email);
        var otpCode = new OtpCode(user.Id, email, codeHash, OtpExpiryMinutes);

        _db.Set<OtpCode>().Add(otpCode);
        await _db.SaveChangesAsync(ct);

        // ۵. Publish به RabbitMQ (نه مستقیم ایمیل)
        var message = new SendOtpEmailMessage
        {
            UserId = user.Id,
            Email = email,
            OtpCode = code, // کد خام فقط در پیام می‌ره، در DB hash ذخیره شده
            ExpiresAt = otpCode.ExpiresAt,
        };

        await _publishEndpoint.Publish(message, ct);

        _logger.LogInformation("OTP sent for user {UserId}", user.Id);

        return (true, "کد تایید ارسال شد.", otpCode.ExpiresAt);
    }

    /// <summary>
    /// تایید OTP و برگرداندن User
    /// </summary>
    public async Task<ApplicationUser?> VerifyOtpAsync(
        string email,
        string code,
        CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        var codeHash = HashCode(code, email);

        var otp = await _db.Set<OtpCode>()
            .Where(o => o.Email == email &&
                        o.CodeHash == codeHash &&
                        !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null || !otp.IsValid())
            return null;

        otp.MarkUsed();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == otp.UserId && u.IsActive, ct);

        await _db.SaveChangesAsync(ct);

        return user;
    }
}
