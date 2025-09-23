using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;

namespace SDIA.Application.PublicAccess.VerifyPhone;

public class PublicAccessVerifyPhoneModel
{
    public string Code { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
}

public class PublicAccessVerifyPhoneResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PublicAccessVerifyPhoneService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessVerifyPhoneService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessVerifyPhoneResult>> ExecuteAsync(
        PublicAccessVerifyPhoneModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r =>
                (r.RegistrationNumber == model.Identifier || r.Phone == model.Identifier) &&
                !r.IsDeleted,
                cancellationToken);

        if (registration == null)
        {
            return Result<PublicAccessVerifyPhoneResult>.NotFound("Registration not found");
        }

        if (registration.PhoneVerified)
        {
            return Result<PublicAccessVerifyPhoneResult>.Success(new PublicAccessVerifyPhoneResult
            {
                Success = true,
                Message = "Phone already verified"
            });
        }

        if (registration.SmsVerificationCode != model.Code)
        {
            return Result<PublicAccessVerifyPhoneResult>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Code", ErrorMessage = "Invalid verification code" }
            });
        }

        if (registration.VerificationCodeExpiry < DateTime.UtcNow)
        {
            return Result<PublicAccessVerifyPhoneResult>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Code", ErrorMessage = "Verification code expired" }
            });
        }

        registration.PhoneVerified = true;
        registration.SmsVerificationCode = string.Empty;
        registration.VerificationCodeExpiry = null;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result<PublicAccessVerifyPhoneResult>.Success(new PublicAccessVerifyPhoneResult
        {
            Success = true,
            Message = "Phone verified successfully"
        });
    }
}