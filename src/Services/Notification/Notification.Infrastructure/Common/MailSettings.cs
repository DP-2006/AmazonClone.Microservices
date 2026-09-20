namespace Notification.Infrastructure.Common;

public sealed class MailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "AmazonClone";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
