using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Management.GenerateAccessLink;

public class RegistrationManagementGenerateAccessLinkService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementGenerateAccessLinkService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementGenerateAccessLinkResult>> ExecuteAsync(
        RegistrationManagementGenerateAccessLinkModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .Include(r => r.Organization)
            .FirstOrDefaultAsync(r => r.Id == model.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result<RegistrationManagementGenerateAccessLinkResult>.NotFound("Inscription non trouvée");
        }

        // Generate access token and verification code
        var accessToken = GenerateSecureToken();
        var verificationCode = GenerateVerificationCode();

        // Update registration
        registration.AccessToken = accessToken;
        registration.AccessTokenExpiry = DateTime.UtcNow.AddHours(24);
        registration.SmsVerificationCode = verificationCode;
        registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        registration.VerificationAttempts = 0;
        registration.LastReminderSentAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new RegistrationManagementGenerateAccessLinkResult
        {
            Success = true,
            AccessToken = accessToken,
            AccessLink = model.BaseUrl + $"/registration-access/{accessToken}",
            VerificationCode = verificationCode,
            ExpiresAt = registration.AccessTokenExpiry.Value,
            NotificationType = !string.IsNullOrEmpty(registration.Phone) ? "SMS" : "Email",
            Recipient = !string.IsNullOrEmpty(registration.Phone) ? registration.Phone : registration.Email,
            RegistrationName = $"{registration.FirstName} {registration.LastName}"
        };

        return Result<RegistrationManagementGenerateAccessLinkResult>.Success(result);
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

public class RegistrationManagementGenerateAccessLinkModel
{
    public Guid RegistrationId { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:5173";
    public bool SendNotification { get; set; } = true;
    public string? CustomMessage { get; set; }
}

public class RegistrationManagementGenerateAccessLinkResult
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string AccessLink { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string RegistrationName { get; set; } = string.Empty;
}