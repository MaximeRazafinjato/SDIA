using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Registrations;
using SDIA.Core.Users;

namespace SDIA.Application.Registrations.Management.Assign;

public class RegistrationManagementAssignValidator : IValidator
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationManagementAssignValidator(
        IRegistrationRepository registrationRepository,
        IUserRepository userRepository)
    {
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not RegistrationManagementAssignModel assignModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Check if registration exists
        if (assignModel.RegistrationId == Guid.Empty)
        {
            validationErrors.AddErrorIfEmpty(assignModel.RegistrationId, nameof(assignModel.RegistrationId), "Registration ID is required");
        }
        else
        {
            var registration = await _registrationRepository.GetByIdAsync(assignModel.RegistrationId, cancellationToken);
            if (registration == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(assignModel.RegistrationId),
                    ErrorMessage = "Registration not found"
                });
            }
        }

        // Check if user exists (if assigning to someone)
        if (assignModel.AssignedToUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(assignModel.AssignedToUserId.Value, cancellationToken);
            if (user == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(assignModel.AssignedToUserId),
                    ErrorMessage = "User not found"
                });
            }
            else if (!user.IsActive)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(assignModel.AssignedToUserId),
                    ErrorMessage = "Cannot assign to inactive user"
                });
            }
        }

        // Validate notes length if provided
        if (!string.IsNullOrEmpty(assignModel.Notes))
        {
            validationErrors.AddErrorIfExceedsLength(assignModel.Notes, 1000, nameof(assignModel.Notes), "Notes must not exceed 1000 characters");
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}