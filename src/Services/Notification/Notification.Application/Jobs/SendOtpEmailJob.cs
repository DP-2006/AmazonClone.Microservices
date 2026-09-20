using Microsoft.Extensions.Logging;
using Notification.Infrastructure.Services;

namespace Notification.Application.Jobs;

public sealed class SendOtpEmailJob
{
    private readonly MailService _mailService;
    private readonly ILogger<SendOtpEmailJob> _logger;

    public SendOtpEmailJob(MailService mailService, ILogger<SendOtpEmailJob> logger)
    {
        _mailService = mailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(string email, string code, DateTime expiresAt, CancellationToken ct)
    {
        _logger.LogInformation("Sending OTP email to {Email}", email);
        var html = $@"
<!DOCTYPE html>
<html dir='rtl' lang='fa'>
<head><meta charset='UTF-8'></head>
<body style='font-family: Tahoma, Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
  <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; padding: 30px;'>
    <h2 style='color: #ff9900;'>AmazonClone</h2>
    <p>کد تایید شما:</p>
    <div style='font-size: 36px; font-weight: bold; letter-spacing: 8px; background: #f0f0f0; padding: 20px; text-align: center; border-radius: 8px; direction: ltr;'>
      {code}
    </div>
    <p style='color: #666; font-size: 14px; margin-top: 20px;'>این کد تا {expiresAt:HH:mm} معتبر است.</p>
    <p style='color: #999; font-size: 12px;'>اگر شما این درخواست را نداده‌اید، این ایمیل را نادیده بگیرید.</p>
  </div>
</body>
</html>";
        await _mailService.SendAsync(email, "کد تایید AmazonClone", html, ct);
    }
}
