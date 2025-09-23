using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;

namespace SDIA.Application.Registrations.Public.GetPublic;

public class RegistrationPublicGetValidator : IValidator
{
    public async Task<Result> ValidateAsync(RegistrationPublicGetModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Token validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Token, nameof(model.Token), "Token is required");

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}
