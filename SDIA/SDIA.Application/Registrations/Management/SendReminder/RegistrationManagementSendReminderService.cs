using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Management.SendReminder;

public class RegistrationManagementSendReminderService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementSendReminderService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementSendReminderResult>> ExecuteAsync(
        RegistrationManagementSendReminderModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r => r.Id == model.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result<RegistrationManagementSendReminderResult>.NotFound("Inscription non trouvée");
        }

        // Generate new access token for the email link
        var token = GenerateSecureToken();
        registration.AccessToken = token;
        registration.AccessTokenExpiry = DateTime.UtcNow.AddDays(7);
        registration.LastReminderSentAt = DateTime.UtcNow;

        // Clear any existing verification code (will be generated when link is clicked)
        registration.SmsVerificationCode = string.Empty;
        registration.VerificationCodeExpiry = null;
        registration.VerificationAttempts = 0;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new RegistrationManagementSendReminderResult
        {
            Success = true,
            Message = "Email de rappel envoyé avec succès",
            AccessLink = model.BaseUrl + $"/registration-access/{token}",
            AccessToken = token,
            ExpiresAt = registration.AccessTokenExpiry.Value,
            NotificationType = "Email",
            Recipient = registration.Email,
            RegistrationName = $"{registration.FirstName} {registration.LastName}"
        };

        return Result<RegistrationManagementSendReminderResult>.Success(result);
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

public class RegistrationManagementSendReminderModel
{
    public Guid RegistrationId { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:5173";
    public string? CustomMessage { get; set; }
}

public class RegistrationManagementSendReminderResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessLink { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string RegistrationName { get; set; } = string.Empty;
}