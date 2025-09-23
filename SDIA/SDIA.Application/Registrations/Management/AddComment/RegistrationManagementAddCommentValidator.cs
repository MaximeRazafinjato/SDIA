using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Registrations;
using SDIA.Core.Users;

namespace SDIA.Application.Registrations.Management.AddComment;

public class RegistrationManagementAddCommentValidator : IValidator
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationManagementAddCommentValidator(
        IRegistrationRepository registrationRepository,
        IUserRepository userRepository)
    {
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not RegistrationManagementAddCommentModel commentModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Validate registration ID
        if (commentModel.RegistrationId == Guid.Empty)
        {
            validationErrors.AddErrorIfEmpty(commentModel.RegistrationId, nameof(commentModel.RegistrationId), "Registration ID is required");
        }
        else
        {
            var registration = await _registrationRepository.GetByIdAsync(commentModel.RegistrationId, cancellationToken);
            if (registration == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(commentModel.RegistrationId),
                    ErrorMessage = "Registration not found"
                });
            }
        }

        // Validate comment
        validationErrors.AddErrorIfNullOrWhiteSpace(commentModel.Comment, nameof(commentModel.Comment), "Comment is required");
        validationErrors.AddErrorIfExceedsLength(commentModel.Comment, 2000, nameof(commentModel.Comment), "Comment must not exceed 2000 characters");

        // Validate user if provided
        if (commentModel.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(commentModel.UserId.Value, cancellationToken);
            if (user == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(commentModel.UserId),
                    ErrorMessage = "User not found"
                });
            }
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}