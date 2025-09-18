using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SDIA.Core.Services;
using SDIA.Infrastructure.Settings;
using System.Net;

namespace SDIA.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SendGridSettings _sendGridSettings;
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SendGridSettings> sendGridSettings,
        ISendGridClient sendGridClient,
        ILogger<EmailService> logger)
    {
        _sendGridSettings = sendGridSettings.Value;
        _sendGridClient = sendGridClient;
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
            var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent ?? "", htmlContent);

            if (!string.IsNullOrEmpty(_sendGridSettings.ReplyToEmail))
            {
                msg.SetReplyTo(new EmailAddress(_sendGridSettings.ReplyToEmail, _sendGridSettings.ReplyToName));
            }

            // Configure tracking
            msg.SetClickTracking(_sendGridSettings.EnableClickTracking, _sendGridSettings.EnableClickTracking);
            msg.SetOpenTracking(_sendGridSettings.EnableOpenTracking);

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {To} with subject: {Subject}", to, subject);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {To}. Status: {Status}, Response: {Response}", 
                    to, response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
            var tos = to.Select(email => new EmailAddress(email)).ToList();
            
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent ?? "", htmlContent);

            if (!string.IsNullOrEmpty(_sendGridSettings.ReplyToEmail))
            {
                msg.SetReplyTo(new EmailAddress(_sendGridSettings.ReplyToEmail, _sendGridSettings.ReplyToName));
            }

            msg.SetClickTracking(_sendGridSettings.EnableClickTracking, _sendGridSettings.EnableClickTracking);
            msg.SetOpenTracking(_sendGridSettings.EnableOpenTracking);

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Bulk email sent successfully to {Count} recipients with subject: {Subject}", 
                    tos.Count, subject);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send bulk email. Status: {Status}, Response: {Response}", 
                    response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending bulk email");
            return false;
        }
    }

    public async Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string htmlContent, IEnumerable<EmailAttachment> attachments)
    {
        try
        {
            var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, "", htmlContent);

            foreach (var attachment in attachments)
            {
                msg.AddAttachment(attachment.FileName, Convert.ToBase64String(attachment.Content), attachment.ContentType);
            }

            if (!string.IsNullOrEmpty(_sendGridSettings.ReplyToEmail))
            {
                msg.SetReplyTo(new EmailAddress(_sendGridSettings.ReplyToEmail, _sendGridSettings.ReplyToName));
            }

            msg.SetClickTracking(_sendGridSettings.EnableClickTracking, _sendGridSettings.EnableClickTracking);
            msg.SetOpenTracking(_sendGridSettings.EnableOpenTracking);

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email with attachments sent successfully to {To}", to);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email with attachments to {To}. Status: {Status}, Response: {Response}", 
                    to, response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email with attachments to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients)
    {
        try
        {
            var personalizations = recipients.Select(recipient =>
            {
                var personalization = new Personalization();
                personalization.Tos = new List<EmailAddress> { new EmailAddress(recipient.Email, recipient.Name) };
                
                foreach (var data in recipient.PersonalizationData)
                {
                    // AddSubstitution method doesn't exist, using Substitutions property
                    if (personalization.Substitutions == null)
                        personalization.Substitutions = new Dictionary<string, string>();
                    personalization.Substitutions.Add($"-{data.Key}-", data.Value?.ToString() ?? "");
                }
                
                return personalization;
            }).ToList();

            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName));
            msg.SetSubject("Bulk Email");
            msg.AddContent(MimeType.Html, "Default content");

            foreach (var personalization in personalizations)
            {
                msg.Personalizations.Add(personalization);
            }

            if (!string.IsNullOrEmpty(_sendGridSettings.ReplyToEmail))
            {
                msg.SetReplyTo(new EmailAddress(_sendGridSettings.ReplyToEmail, _sendGridSettings.ReplyToName));
            }

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Bulk personalized email sent successfully to {Count} recipients", recipients.Count());
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send bulk personalized email. Status: {Status}, Response: {Response}", 
                    response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending bulk personalized email");
            return false;
        }
    }

    public async Task<bool> SendTemplateEmailAsync(string to, string templateId, Dictionary<string, object> templateData)
    {
        try
        {
            var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
            var toEmail = new EmailAddress(to);
            var msg = new SendGridMessage();
            
            msg.SetFrom(from);
            msg.AddTo(toEmail);
            msg.SetTemplateId(templateId);

            var dynamicTemplateData = templateData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            msg.SetTemplateData(dynamicTemplateData);

            if (!string.IsNullOrEmpty(_sendGridSettings.ReplyToEmail))
            {
                msg.SetReplyTo(new EmailAddress(_sendGridSettings.ReplyToEmail, _sendGridSettings.ReplyToName));
            }

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Template email sent successfully to {To} using template {TemplateId}", to, templateId);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send template email to {To}. Status: {Status}, Response: {Response}", 
                    to, response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending template email to {To}", to);
            return false;
        }
    }

    public async Task<EmailDeliveryStatus> GetDeliveryStatusAsync(string messageId)
    {
        // Note: SendGrid's API doesn't provide direct message status lookup via message ID
        // This would typically require webhook implementation or activity API usage
        _logger.LogWarning("GetDeliveryStatusAsync not fully implemented - requires webhook setup");
        
        return new EmailDeliveryStatus
        {
            MessageId = messageId,
            Status = "Unknown",
            Timestamp = DateTime.UtcNow,
            ErrorMessage = "Status lookup not implemented"
        };
    }

    public async Task<IEnumerable<EmailStats>> GetEmailStatsAsync(DateTime startDate, DateTime endDate)
    {
        // Note: This would typically use SendGrid's Statistics API
        _logger.LogWarning("GetEmailStatsAsync not fully implemented - requires Statistics API integration");
        
        return new List<EmailStats>
        {
            new EmailStats
            {
                Date = DateTime.UtcNow.Date,
                Sent = 0,
                Delivered = 0,
                Bounced = 0,
                Opened = 0,
                Clicked = 0
            }
        };
    }
}