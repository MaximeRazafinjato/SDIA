namespace SDIA.Core.Services;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendBulkSmsAsync(IEnumerable<BulkSmsRecipient> recipients);
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId);
    Task<IEnumerable<SmsStats>> GetSmsStatsAsync(DateTime startDate, DateTime endDate);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
    Task<string> SendVerificationCodeAsync(string phoneNumber);
    Task<bool> VerifyCodeAsync(string phoneNumber, string code);
}

public class BulkSmsRecipient
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> PersonalizationData { get; set; } = new();
}

public class SmsDeliveryStatus
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Sent, Delivered, Failed, etc.
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? Cost { get; set; }
}

public class SmsStats
{
    public DateTime Date { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
    public decimal TotalCost { get; set; }
}