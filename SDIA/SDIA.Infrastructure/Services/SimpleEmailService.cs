using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDIA.Core.Services;
using System.Net;
using System.Net.Mail;

namespace SDIA.Infrastructure.Services;

public class SimpleEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SimpleEmailService> _logger;

    public SimpleEmailService(IConfiguration configuration, ILogger<SimpleEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent)
    {
        return await SendEmailAsync(to, subject, htmlContent, null);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            // In development, just log the email
            _logger.LogInformation("=== EMAIL SIMULATION ===");
            _logger.LogInformation("To: {To}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", htmlContent);
            _logger.LogInformation("========================");
            
            // Simulate async operation
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string htmlContent, string? plainTextContent = null)
    {
        var tasks = to.Select(recipient => SendEmailAsync(recipient, subject, htmlContent, plainTextContent));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string htmlContent, IEnumerable<EmailAttachment> attachments)
    {
        _logger.LogInformation("Email with {Count} attachments would be sent to {To}", attachments.Count(), to);
        return await SendEmailAsync(to, subject, htmlContent);
    }

    public async Task<bool> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients)
    {
        var tasks = recipients.Select(r => SendEmailAsync(r.Email, "Bulk Email", $"<p>Hello {r.Name}</p>"));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task<bool> SendTemplateEmailAsync(string to, string templateId, Dictionary<string, object> templateData)
    {
        var content = $"Template: {templateId}";
        return await SendEmailAsync(to, $"Template Email - {templateId}", content);
    }

    public async Task<EmailDeliveryStatus> GetDeliveryStatusAsync(string messageId)
    {
        await Task.Delay(1);
        return new EmailDeliveryStatus
        {
            MessageId = messageId,
            Status = "Delivered",
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<EmailStats>> GetEmailStatsAsync(DateTime startDate, DateTime endDate)
    {
        await Task.Delay(1);
        return new List<EmailStats>
        {
            new EmailStats
            {
                Date = DateTime.UtcNow.Date,
                Sent = 100,
                Delivered = 95,
                Bounced = 5,
                Opened = 80,
                Clicked = 40
            }
        };
    }
}