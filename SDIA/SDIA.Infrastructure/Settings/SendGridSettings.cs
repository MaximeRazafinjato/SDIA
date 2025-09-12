namespace SDIA.Infrastructure.Settings;

public class SendGridSettings
{
    public const string SectionName = "SendGrid";
    
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ReplyToEmail { get; set; } = string.Empty;
    public string ReplyToName { get; set; } = string.Empty;
    public bool EnableClickTracking { get; set; } = true;
    public bool EnableOpenTracking { get; set; } = true;
    public string WebhookUrl { get; set; } = string.Empty;
    public string[] TemplateIds { get; set; } = Array.Empty<string>();
    public int RateLimitPerSecond { get; set; } = 100;
    public int RetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}