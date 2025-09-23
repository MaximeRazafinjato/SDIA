using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.PublicAccess.VerifyCode;

public class PublicAccessVerifyCodeService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessVerifyCodeService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessVerifyCodeResult>> ExecuteAsync(
        PublicAccessVerifyCodeModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByAccessTokenAsync(model.Token, cancellationToken);

        if (registration == null)
        {
            return Result<PublicAccessVerifyCodeResult>.NotFound("Lien d'accès invalide");
        }

        if (registration.AccessTokenExpiry < DateTime.UtcNow)
        {
            return Result<PublicAccessVerifyCodeResult>.Error("Ce lien a expiré. Veuillez demander un nouveau lien.");
        }

        if (registration.VerificationAttempts >= 5)
        {
            return Result<PublicAccessVerifyCodeResult>.Error("Trop de tentatives échouées. Veuillez demander un nouveau code.");
        }

        // Increment attempts
        registration.VerificationAttempts++;

        // Check if code matches
        if (registration.SmsVerificationCode != model.Code)
        {
            await _registrationRepository.UpdateAsync(registration, cancellationToken);
            return Result<PublicAccessVerifyCodeResult>.Invalid(new ValidationError
            {
                Identifier = "Code",
                ErrorMessage = $"Code de vérification incorrect. {5 - registration.VerificationAttempts} tentatives restantes"
            });
        }

        // Check if code is expired
        if (registration.VerificationCodeExpiry < DateTime.UtcNow)
        {
            await _registrationRepository.UpdateAsync(registration, cancellationToken);
            return Result<PublicAccessVerifyCodeResult>.Error("Le code de vérification a expiré");
        }

        // Code is valid - mark phone as verified
        registration.PhoneVerified = true;
        registration.SmsVerificationCode = string.Empty;
        registration.VerificationCodeExpiry = null;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new PublicAccessVerifyCodeResult
        {
            Success = true,
            Message = "Vérification réussie",
            RegistrationId = registration.Id,
            SessionToken = GenerateSessionToken()
        };

        return Result<PublicAccessVerifyCodeResult>.Success(result);
    }

    private static string GenerateSessionToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}