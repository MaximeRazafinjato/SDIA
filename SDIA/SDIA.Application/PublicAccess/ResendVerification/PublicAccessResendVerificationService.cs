using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.PublicAccess.ResendVerification;

public class PublicAccessResendVerificationService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessResendVerificationService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessResendVerificationResult>> ExecuteAsync(
        PublicAccessResendVerificationModel model,
        CancellationToken cancellationToken = default)
    {
        // Find registration by access token
        var registration = await _registrationRepository.GetByTokenAsync(model.Token);

        if (registration == null)
        {
            return Result<PublicAccessResendVerificationResult>.NotFound("Lien d'accès invalide");
        }

        // Check if token is expired
        if (registration.AccessTokenExpiry < DateTime.UtcNow)
        {
            return Result<PublicAccessResendVerificationResult>.Error("Ce lien a expiré. Veuillez demander un nouveau lien.");
        }

        // Check if phone number exists
        if (string.IsNullOrEmpty(registration.Phone))
        {
            var result = new PublicAccessResendVerificationResult
            {
                Success = false,
                Message = "Aucun numéro de téléphone n'est associé à cette inscription",
                RequiresPhoneUpdate = true
            };
            return Result<PublicAccessResendVerificationResult>.Success(result);
        }

        // Generate new verification code
        var verificationCode = GenerateVerificationCode();
        registration.SmsVerificationCode = verificationCode;
        registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        registration.VerificationAttempts = 0;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // TODO: Send SMS with verification code
        // This should be handled by a notification service

        var maskedPhone = MaskPhoneNumber(registration.Phone);
        var successResult = new PublicAccessResendVerificationResult
        {
            Success = true,
            Message = $"Un code de vérification a été envoyé au {maskedPhone}",
            PhoneNumber = maskedPhone,
            ExpiresInMinutes = 10,
            RequiresPhoneUpdate = false
        };

        return Result<PublicAccessResendVerificationResult>.Success(successResult);
    }

    private static string GenerateVerificationCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3];
        rng.GetBytes(bytes);
        var code = BitConverter.ToUInt32(new byte[] { bytes[0], bytes[1], bytes[2], 0 }, 0) % 1000000;
        return code.ToString("D6");
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";

        return phone.Substring(0, 2) + new string('*', phone.Length - 4) + phone.Substring(phone.Length - 2);
    }
}