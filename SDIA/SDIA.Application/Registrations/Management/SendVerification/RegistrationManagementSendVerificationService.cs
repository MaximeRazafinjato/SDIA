using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Management.SendVerification;

public class RegistrationManagementSendVerificationService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementSendVerificationService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementSendVerificationResult>> ExecuteAsync(
        RegistrationManagementSendVerificationModel model,
        CancellationToken cancellationToken = default)
    {
        // Get the registration
        var registration = await _registrationRepository.GetByIdAsync(model.RegistrationId, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationManagementSendVerificationResult>.NotFound("Registration not found");
        }

        var result = new RegistrationManagementSendVerificationResult
        {
            VerificationType = model.VerificationType,
            SentAt = DateTime.UtcNow,
            ExpiresInMinutes = model.VerificationType == "email" ? 60 : 10
        };

        if (model.VerificationType.ToLower() == "email")
        {
            // Validate email exists
            if (string.IsNullOrEmpty(registration.Email))
            {
                return Result<RegistrationManagementSendVerificationResult>.Error("No email address associated with this registration");
            }

            // Generate email verification token
            var emailToken = GenerateEmailVerificationToken();
            registration.EmailVerificationToken = emailToken;
            registration.UpdatedAt = DateTime.UtcNow;

            await _registrationRepository.UpdateAsync(registration, cancellationToken);

            // TODO: Send email verification
            result.Success = true;
            result.Message = $"Email verification sent to {MaskEmail(registration.Email)}";
            result.MaskedContact = MaskEmail(registration.Email);
        }
        else if (model.VerificationType.ToLower() == "sms")
        {
            // Validate phone exists
            if (string.IsNullOrEmpty(registration.Phone))
            {
                return Result<RegistrationManagementSendVerificationResult>.Error("No phone number associated with this registration");
            }

            // Generate SMS verification code
            var smsCode = GenerateSmsVerificationCode();
            registration.SmsVerificationCode = smsCode;
            registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            registration.VerificationAttempts = 0;
            registration.UpdatedAt = DateTime.UtcNow;

            await _registrationRepository.UpdateAsync(registration, cancellationToken);

            // TODO: Send SMS verification
            result.Success = true;
            result.Message = $"SMS verification code sent to {MaskPhoneNumber(registration.Phone)}";
            result.MaskedContact = MaskPhoneNumber(registration.Phone);
        }
        else
        {
            return Result<RegistrationManagementSendVerificationResult>.Error("Invalid verification type. Must be 'email' or 'sms'");
        }

        return Result<RegistrationManagementSendVerificationResult>.Success(result);
    }

    private static string GenerateEmailVerificationToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string GenerateSmsVerificationCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3];
        rng.GetBytes(bytes);
        var code = BitConverter.ToUInt32(new byte[] { bytes[0], bytes[1], bytes[2], 0 }, 0) % 1000000;
        return code.ToString("D6");
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "****@****.***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domainPart = parts[1];

        var maskedLocal = localPart.Length > 2
            ? $"{localPart[0]}***{localPart[^1]}"
            : "****";

        var maskedDomain = domainPart.Length > 4
            ? $"{domainPart.Substring(0, 2)}***{domainPart.Substring(domainPart.Length - 3)}"
            : "****";

        return $"{maskedLocal}@{maskedDomain}";
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";

        return phone.Substring(0, 2) + new string('*', phone.Length - 4) + phone.Substring(phone.Length - 2);
    }
}