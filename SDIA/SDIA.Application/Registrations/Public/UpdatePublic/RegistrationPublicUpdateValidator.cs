using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;

namespace SDIA.Application.Registrations.Public.UpdatePublic;

public class RegistrationPublicUpdateValidator : IValidator
{
    public async Task<Result> ValidateAsync(RegistrationPublicUpdateModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Token validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Token, nameof(model.Token), "Token is required");

        // FirstName validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.FirstName, nameof(model.FirstName), "First name is required");
        validationErrors.AddErrorIfExceedsLength(model.FirstName, 100, nameof(model.FirstName), "First name must not exceed 100 characters");

        // LastName validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.LastName, nameof(model.LastName), "Last name is required");
        validationErrors.AddErrorIfExceedsLength(model.LastName, 100, nameof(model.LastName), "Last name must not exceed 100 characters");

        // Email validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Email, nameof(model.Email), "Email is required");
        validationErrors.AddErrorIfExceedsLength(model.Email, 255, nameof(model.Email), "Email must not exceed 255 characters");
        validationErrors.AddErrorIfNotEmail(model.Email, nameof(model.Email), "Invalid email format");

        // Phone validation (optional)
        if (!string.IsNullOrWhiteSpace(model.Phone))
        {
            validationErrors.AddErrorIfNotMatch(model.Phone, @"^\+?[1-9]\d{1,14}$", nameof(model.Phone), "Invalid phone format");
        }

        // BirthDate validation
        if (model.BirthDate == default)
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.BirthDate), ErrorMessage = "Birth date is required" });
        }
        else if (model.BirthDate > DateTime.UtcNow)
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.BirthDate), ErrorMessage = "Birth date cannot be in the future" });
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}
