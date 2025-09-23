using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Public.VerifyAccess;

public class RegistrationPublicVerifyAccessService
{
    private readonly RegistrationPublicVerifyAccessValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicVerifyAccessService(
        RegistrationPublicVerifyAccessValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicVerifyAccessResult>> ExecuteAsync(
        RegistrationPublicVerifyAccessModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationPublicVerifyAccessResult>.Invalid(validationResult.ValidationErrors);
        }

        // Find registration by access token
        var registration = await _registrationRepository.GetByAccessTokenAsync(model.Token, cancellationToken);
        
        if (registration == null)
        {
            return Result<RegistrationPublicVerifyAccessResult>.Error("Invalid or expired link");
        }

        // Check token expiry
        if (registration.AccessTokenExpiry < DateTime.UtcNow)
        {
            return Result<RegistrationPublicVerifyAccessResult>.Error("This link has expired. Please request a new link.");
        }

        // Check verification attempts
        if (registration.VerificationAttempts >= 3)
        {
            return Result<RegistrationPublicVerifyAccessResult>.Error("Too many attempts. Please request a new link.");
        }

        // Verify code
        if (registration.SmsVerificationCode != model.Code)
        {
            registration.VerificationAttempts++;
            await _registrationRepository.UpdateAsync(registration, cancellationToken);

            var remainingAttempts = 3 - registration.VerificationAttempts;
            return Result<RegistrationPublicVerifyAccessResult>.Error(
                $"Incorrect code. {remainingAttempts} attempt(s) remaining.");
        }

        // Check code expiry
        if (registration.VerificationCodeExpiry < DateTime.UtcNow)
        {
            return Result<RegistrationPublicVerifyAccessResult>.Error("This code has expired. Please request a new code.");
        }

        // Reset attempts on successful verification
        registration.VerificationAttempts = 0;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // Generate session token (valid for 4 hours)
        var sessionToken = GenerateSecureToken();

        var result = new RegistrationPublicVerifyAccessResult
        {
            Success = true,
            SessionToken = sessionToken,
            RegistrationId = registration.Id,
            Message = "Verification successful"
        };

        return Result<RegistrationPublicVerifyAccessResult>.Success(result);
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
