namespace Identity.Application.Common;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "AmazonClone";
    public string Audience { get; set; } = "AmazonClone.Client";
    public string Secret { get; set; } = "THIS_IS_A_VERY_LONG_SECRET_KEY_FOR_JWT_AT_LEAST_32_CHARS";
    public int ExpiryMinutes { get; set; } = 60;
}
