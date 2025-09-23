using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.Validate;

public class RegistrationManagementValidateValidator : IValidator
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementValidateValidator(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not RegistrationManagementValidateModel validateModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Check if registration exists
        if (validateModel.RegistrationId == Guid.Empty)
        {
            validationErrors.AddErrorIfEmpty(validateModel.RegistrationId, nameof(validateModel.RegistrationId), "Registration ID is required");
        }
        else
        {
            var registration = await _registrationRepository.GetByIdAsync(validateModel.RegistrationId, cancellationToken);
            if (registration == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(validateModel.RegistrationId),
                    ErrorMessage = "Registration not found"
                });
            }
            else
            {
                // Check if registration is in a valid state for validation
                if (registration.Status != SDIA.SharedKernel.Enums.RegistrationStatus.Pending)
                {
                    validationErrors.Add(new Ardalis.Result.ValidationError
                    {
                        Identifier = nameof(validateModel.RegistrationId),
                        ErrorMessage = "Only pending registrations can be validated"
                    });
                }
            }
        }

        // If rejecting, rejection reason is required
        if (!validateModel.IsApproved)
        {
            validationErrors.AddErrorIfNullOrWhiteSpace(validateModel.RejectionReason, nameof(validateModel.RejectionReason), "Rejection reason is required when rejecting a registration");

            if (!string.IsNullOrEmpty(validateModel.RejectionReason))
            {
                validationErrors.AddErrorIfExceedsLength(validateModel.RejectionReason, 1000, nameof(validateModel.RejectionReason), "Rejection reason must not exceed 1000 characters");
            }
        }

        // Validate notes length if provided
        if (!string.IsNullOrEmpty(validateModel.Notes))
        {
            validationErrors.AddErrorIfExceedsLength(validateModel.Notes, 1000, nameof(validateModel.Notes), "Notes must not exceed 1000 characters");
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}