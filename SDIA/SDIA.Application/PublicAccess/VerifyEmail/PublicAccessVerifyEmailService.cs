using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;

namespace SDIA.Application.PublicAccess.VerifyEmail;

public class PublicAccessVerifyEmailService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessVerifyEmailService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessVerifyEmailResult>> ExecuteAsync(
        PublicAccessVerifyEmailModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r =>
                (r.RegistrationNumber == model.Identifier || r.Email == model.Identifier) &&
                !r.IsDeleted,
                cancellationToken);

        if (registration == null)
        {
            return Result<PublicAccessVerifyEmailResult>.NotFound("Registration not found");
        }

        if (registration.EmailVerified)
        {
            return Result<PublicAccessVerifyEmailResult>.Success(new PublicAccessVerifyEmailResult
            {
                Success = true,
                Message = "Email already verified"
            });
        }

        if (registration.EmailVerificationToken != model.Code)
        {
            return Result<PublicAccessVerifyEmailResult>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Code", ErrorMessage = "Invalid verification code" }
            });
        }

        if (registration.VerificationCodeExpiry < DateTime.UtcNow)
        {
            return Result<PublicAccessVerifyEmailResult>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Code", ErrorMessage = "Verification code expired" }
            });
        }

        registration.EmailVerified = true;
        registration.EmailVerificationToken = string.Empty;
        registration.VerificationCodeExpiry = null;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result<PublicAccessVerifyEmailResult>.Success(new PublicAccessVerifyEmailResult
        {
            Success = true,
            Message = "Email verified successfully"
        });
    }
}