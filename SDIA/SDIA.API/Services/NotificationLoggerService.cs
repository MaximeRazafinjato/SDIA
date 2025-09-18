using System.IO;

namespace SDIA.API.Services;

public interface INotificationLoggerService
{
    Task LogSmsNotification(string phoneNumber, string name, string code, string link, string message);
    Task LogEmailNotification(string email, string name, string code, string link, string message);
}

public class NotificationLoggerService : INotificationLoggerService
{
    private readonly ILogger<NotificationLoggerService> _logger;
    private readonly string _logDirectory;

    public NotificationLoggerService(ILogger<NotificationLoggerService> logger)
    {
        _logger = logger;
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "notifications");

        // Ensure the directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public async Task LogSmsNotification(string phoneNumber, string name, string code, string link, string message)
    {
        // Add milliseconds and a random component to prevent file name collisions
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var fileName = $"{timestamp}_{randomSuffix}_sms.log";
        var filePath = Path.Combine(_logDirectory, fileName);

        var logContent = $@"=== NOTIFICATION SMS ===
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Destinataire: {phoneNumber}
Nom: {name}
Code de vérification: {code}
Lien d'accès: {link}
Expiration du lien: {DateTime.Now.AddHours(24):yyyy-MM-dd HH:mm:ss}
Message:
{message}
========================
";

        // Retry logic in case of file access issues
        int retries = 3;
        for (int i = 0; i < retries; i++)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, logContent);
                _logger.LogInformation($"SMS notification logged to {fileName}");
                break;
            }
            catch (IOException) when (i < retries - 1)
            {
                await Task.Delay(100);
                // Generate a new file name for the retry
                randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
                fileName = $"{timestamp}_{randomSuffix}_sms.log";
                filePath = Path.Combine(_logDirectory, fileName);
            }
        }
    }

    public async Task LogEmailNotification(string email, string name, string code, string link, string message)
    {
        // Add milliseconds and a random component to prevent file name collisions
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var fileName = $"{timestamp}_{randomSuffix}_email.log";
        var filePath = Path.Combine(_logDirectory, fileName);

        var logContent = $@"=== NOTIFICATION EMAIL ===
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Destinataire: {email}
Nom: {name}
Code de vérification: {code}
Lien d'accès: {link}
Expiration du lien: {DateTime.Now.AddHours(24):yyyy-MM-dd HH:mm:ss}
Message:
{message}
==========================
";

        // Retry logic in case of file access issues
        int retries = 3;
        for (int i = 0; i < retries; i++)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, logContent);
                _logger.LogInformation($"Email notification logged to {fileName}");
                break;
            }
            catch (IOException) when (i < retries - 1)
            {
                await Task.Delay(100);
                // Generate a new file name for the retry
                randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
                fileName = $"{timestamp}_{randomSuffix}_email.log";
                filePath = Path.Combine(_logDirectory, fileName);
            }
        }
    }
}