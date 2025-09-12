namespace SDIA.Core.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlContent);
    Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string htmlContent, IEnumerable<EmailAttachment> attachments);
    Task<bool> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients);
    Task<bool> SendTemplateEmailAsync(string to, string templateId, Dictionary<string, object> templateData);
    Task<EmailDeliveryStatus> GetDeliveryStatusAsync(string messageId);
    Task<IEnumerable<EmailStats>> GetEmailStatsAsync(DateTime startDate, DateTime endDate);
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class BulkEmailRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> PersonalizationData { get; set; } = new();
}

public class EmailDeliveryStatus
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Sent, Delivered, Bounced, etc.
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

public class EmailStats
{
    public DateTime Date { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Bounced { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
}