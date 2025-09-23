using Ardalis.Result;
using SDIA.Core.Registrations;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Public.VerifyCode;

public class RegistrationPublicVerifyCodeService
{
    private readonly RegistrationPublicVerifyCodeValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicVerifyCodeService(
        RegistrationPublicVerifyCodeValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicVerifyCodeResult>> ExecuteAsync(
        RegistrationPublicVerifyCodeModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationPublicVerifyCodeResult>.Invalid(validationResult.ValidationErrors);
        }

        // Find registration by access token
        var registration = await _registrationRepository.GetByTokenAsync(model.AccessToken);
        if (registration == null)
        {
            return Result<RegistrationPublicVerifyCodeResult>.NotFound("Invalid access token");
        }

        // Check if token is expired
        if (registration.AccessTokenExpiry < DateTime.UtcNow)
        {
            return Result<RegistrationPublicVerifyCodeResult>.Error("Access token has expired");
        }

        // Check verification attempts
        if (registration.VerificationAttempts >= 5)
        {
            return Result<RegistrationPublicVerifyCodeResult>.Success(new RegistrationPublicVerifyCodeResult
            {
                Success = false,
                Message = "Too many failed attempts. Please request a new code.",
                MaxAttemptsReached = true,
                AttemptsRemaining = 0
            });
        }

        // Increment attempts
        registration.VerificationAttempts++;

        // Check if code matches
        if (registration.SmsVerificationCode != model.Code)
        {
            await _registrationRepository.UpdateAsync(registration, cancellationToken);

            return Result<RegistrationPublicVerifyCodeResult>.Success(new RegistrationPublicVerifyCodeResult
            {
                Success = false,
                Message = "Invalid verification code",
                AttemptsRemaining = 5 - registration.VerificationAttempts,
                MaxAttemptsReached = false
            });
        }

        // Check if code is expired
        if (registration.VerificationCodeExpiry < DateTime.UtcNow)
        {
            await _registrationRepository.UpdateAsync(registration, cancellationToken);

            return Result<RegistrationPublicVerifyCodeResult>.Success(new RegistrationPublicVerifyCodeResult
            {
                Success = false,
                Message = "Verification code has expired",
                AttemptsRemaining = 5 - registration.VerificationAttempts
            });
        }

        // Code is valid - mark phone as verified
        registration.PhoneVerified = true;
        registration.SmsVerificationCode = string.Empty;
        registration.VerificationCodeExpiry = null;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // Generate a session token for editing
        var sessionToken = GenerateSessionToken();

        var result = new RegistrationPublicVerifyCodeResult
        {
            Success = true,
            Message = "Verification successful",
            RegistrationId = registration.Id,
            SessionToken = sessionToken,
            RedirectUrl = $"/registration-edit/{registration.Id}?session={sessionToken}",
            AttemptsRemaining = 0,
            MaxAttemptsReached = false
        };

        return Result<RegistrationPublicVerifyCodeResult>.Success(result);
    }

    private static string GenerateSessionToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}