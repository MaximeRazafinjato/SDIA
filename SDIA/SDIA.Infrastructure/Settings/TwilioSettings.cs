namespace SDIA.Infrastructure.Settings;

public class TwilioSettings
{
    public const string SectionName = "Twilio";
    
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
    public string MessagingServiceSid { get; set; } = string.Empty;
    public bool EnableDeliveryReports { get; set; } = true;
    public string WebhookUrl { get; set; } = string.Empty;
    public int RateLimitPerSecond { get; set; } = 10;
    public int RetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan VerificationCodeExpiry { get; set; } = TimeSpan.FromMinutes(10);
    public string[] AllowedCountries { get; set; } = Array.Empty<string>();
}