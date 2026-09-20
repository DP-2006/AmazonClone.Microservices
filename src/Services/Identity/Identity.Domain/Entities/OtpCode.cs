namespace Identity.Domain.Entities;

/// <summary>
/// کد OTP برای ورود بدون رمز
/// یک کاربر می‌تونه چند کد داشته باشه ولی فقط آخرین کد معتبره
/// </summary>
public sealed class OtpCode
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private OtpCode() { }

    public OtpCode(Guid userId, string email, string codeHash, int expiryMinutes = 5)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email required.");
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException("CodeHash required.");

        UserId = userId;
        Email = email.Trim().ToLowerInvariant();
        CodeHash = codeHash;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt;
    }

    public void MarkUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
