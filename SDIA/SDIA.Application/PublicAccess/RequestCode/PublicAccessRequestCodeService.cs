using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.PublicAccess.RequestCode;

public class PublicAccessRequestCodeService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessRequestCodeService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessRequestCodeResult>> ExecuteAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByAccessTokenAsync(token, cancellationToken);

        if (registration == null)
        {
            return Result<PublicAccessRequestCodeResult>.NotFound("Lien d'accès invalide");
        }

        if (registration.AccessTokenExpiry < DateTime.UtcNow)
        {
            return Result<PublicAccessRequestCodeResult>.Error("Ce lien a expiré. Veuillez demander un nouveau lien.");
        }

        if (string.IsNullOrEmpty(registration.Phone))
        {
            return Result<PublicAccessRequestCodeResult>.Invalid(new ValidationError
            {
                Identifier = "Phone",
                ErrorMessage = "Aucun numéro de téléphone n'est associé à cette inscription"
            });
        }

        // Generate new verification code
        var verificationCode = GenerateVerificationCode();
        registration.SmsVerificationCode = verificationCode;
        registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        registration.VerificationAttempts = 0;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new PublicAccessRequestCodeResult
        {
            VerificationCode = verificationCode,
            PhoneNumber = registration.Phone,
            MaskedPhone = MaskPhoneNumber(registration.Phone),
            FullName = $"{registration.FirstName} {registration.LastName}",
            ExpiresInMinutes = 10
        };

        return Result<PublicAccessRequestCodeResult>.Success(result);
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