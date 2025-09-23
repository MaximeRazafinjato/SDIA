using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Public.VerifyAccess;

public class RegistrationPublicVerifyAccessValidator : IValidator
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicVerifyAccessValidator(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ValidateAsync(RegistrationPublicVerifyAccessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Token validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Token, nameof(model.Token), "Token is required");
        
        // Code validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Code, nameof(model.Code), "Verification code is required");
        if (!string.IsNullOrWhiteSpace(model.Code))
        {
            validationErrors.AddErrorIfNotMatch(model.Code, @"^\d{6}$", nameof(model.Code), "Code must be 6 digits");
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}
