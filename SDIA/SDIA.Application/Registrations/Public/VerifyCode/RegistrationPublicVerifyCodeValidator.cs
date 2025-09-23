using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Public.VerifyCode;

public class RegistrationPublicVerifyCodeValidator : IValidator
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicVerifyCodeValidator(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not RegistrationPublicVerifyCodeModel verifyModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Validate access token
        validationErrors.AddErrorIfNullOrWhiteSpace(verifyModel.AccessToken, nameof(verifyModel.AccessToken), "Access token is required");

        // Validate code
        validationErrors.AddErrorIfNullOrWhiteSpace(verifyModel.Code, nameof(verifyModel.Code), "Verification code is required");

        if (!string.IsNullOrWhiteSpace(verifyModel.Code) && verifyModel.Code.Length != 6)
        {
            validationErrors.Add(new Ardalis.Result.ValidationError
            {
                Identifier = nameof(verifyModel.Code),
                ErrorMessage = "Verification code must be 6 digits"
            });
        }

        // Check if registration exists with this token
        if (!string.IsNullOrWhiteSpace(verifyModel.AccessToken))
        {
            var registration = await _registrationRepository.GetByTokenAsync(verifyModel.AccessToken);
            if (registration == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(verifyModel.AccessToken),
                    ErrorMessage = "Invalid access token"
                });
            }
            else
            {
                // Check if token is expired
                if (registration.AccessTokenExpiry < DateTime.UtcNow)
                {
                    validationErrors.Add(new Ardalis.Result.ValidationError
                    {
                        Identifier = nameof(verifyModel.AccessToken),
                        ErrorMessage = "Access token has expired"
                    });
                }
            }
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}