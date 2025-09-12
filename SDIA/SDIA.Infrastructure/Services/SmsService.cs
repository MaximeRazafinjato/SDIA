using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDIA.Core.Services;
using SDIA.Infrastructure.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace SDIA.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly TwilioSettings _twilioSettings;
    private readonly ILogger<SmsService> _logger;
    private readonly Dictionary<string, string> _verificationCodes;

    public SmsService(
        IOptions<TwilioSettings> twilioSettings,
        ILogger<SmsService> logger)
    {
        _twilioSettings = twilioSettings.Value;
        _logger = logger;
        _verificationCodes = new Dictionary<string, string>();

        // Initialize Twilio
        TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_twilioSettings.FromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            if (messageResource.ErrorCode == null)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {Sid}", 
                    phoneNumber, messageResource.Sid);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send SMS to {PhoneNumber}. Error: {Error}", 
                    phoneNumber, messageResource.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendBulkSmsAsync(IEnumerable<BulkSmsRecipient> recipients)
    {
        var tasks = recipients.Select(async recipient =>
        {
            try
            {
                var personalizedMessage = recipient.Message;
                
                // Replace personalization data in the message
                foreach (var data in recipient.PersonalizationData)
                {
                    personalizedMessage = personalizedMessage.Replace($"{{{data.Key}}}", data.Value?.ToString() ?? "");
                }

                return await SendSmsAsync(recipient.PhoneNumber, personalizedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", recipient.PhoneNumber);
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r);
        
        _logger.LogInformation("Bulk SMS completed. {Success}/{Total} messages sent successfully", 
            successCount, recipients.Count());

        return successCount > 0;
    }

    public async Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId)
    {
        try
        {
            var message = await MessageResource.FetchAsync(messageId);
            
            return new SmsDeliveryStatus
            {
                MessageId = message.Sid,
                Status = message.Status.ToString(),
                Timestamp = message.DateSent ?? DateTime.UtcNow,
                ErrorMessage = message.ErrorMessage,
                Cost = decimal.TryParse(message.Price, out var price) ? price : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting SMS delivery status for {MessageId}", messageId);
            return new SmsDeliveryStatus
            {
                MessageId = messageId,
                Status = "Unknown",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IEnumerable<SmsStats>> GetSmsStatsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var messages = await MessageResource.ReadAsync(
                dateSentAfter: startDate,
                dateSentBefore: endDate,
                limit: 1000
            );

            var groupedStats = messages
                .Where(m => m.DateSent.HasValue)
                .GroupBy(m => m.DateSent.Value.Date)
                .Select(g => new SmsStats
                {
                    Date = g.Key,
                    Sent = g.Count(),
                    Delivered = g.Count(m => m.Status == MessageResource.StatusEnum.Delivered),
                    Failed = g.Count(m => m.Status == MessageResource.StatusEnum.Failed || 
                                         m.Status == MessageResource.StatusEnum.Undelivered),
                    TotalCost = g.Sum(m => decimal.TryParse(m.Price, out var price) ? Math.Abs(price) : 0)
                })
                .OrderBy(s => s.Date);

            return groupedStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting SMS stats");
            return new List<SmsStats>();
        }
    }

    public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
    {
        try
        {
            // Basic validation - you might want to use Twilio's Lookup API for more thorough validation
            var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var parsedNumber = phoneUtil.Parse(phoneNumber, null);
            return phoneUtil.IsValidNumber(parsedNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating phone number {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<string> SendVerificationCodeAsync(string phoneNumber)
    {
        try
        {
            // Generate a random 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            
            var message = $"Your verification code is: {code}. This code will expire in {_twilioSettings.VerificationCodeExpiry.TotalMinutes} minutes.";
            
            var success = await SendSmsAsync(phoneNumber, message);
            
            if (success)
            {
                // Store the code with expiration (in a real implementation, use a proper cache like Redis)
                var key = $"{phoneNumber}_{DateTime.UtcNow.Ticks}";
                _verificationCodes[phoneNumber] = $"{code}_{DateTime.UtcNow.Add(_twilioSettings.VerificationCodeExpiry).Ticks}";
                
                _logger.LogInformation("Verification code sent to {PhoneNumber}", phoneNumber);
                return key; // Return a tracking ID
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending verification code to {PhoneNumber}", phoneNumber);
            return string.Empty;
        }
    }

    public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
    {
        try
        {
            if (_verificationCodes.TryGetValue(phoneNumber, out var storedData))
            {
                var parts = storedData.Split('_');
                if (parts.Length == 2)
                {
                    var storedCode = parts[0];
                    var expirationTicks = long.Parse(parts[1]);
                    var expiration = new DateTime(expirationTicks);
                    
                    if (DateTime.UtcNow <= expiration && storedCode == code)
                    {
                        _verificationCodes.Remove(phoneNumber); // Remove used code
                        _logger.LogInformation("Verification code verified successfully for {PhoneNumber}", phoneNumber);
                        return true;
                    }
                    else if (DateTime.UtcNow > expiration)
                    {
                        _verificationCodes.Remove(phoneNumber); // Remove expired code
                        _logger.LogWarning("Verification code expired for {PhoneNumber}", phoneNumber);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid verification code provided for {PhoneNumber}", phoneNumber);
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while verifying code for {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}