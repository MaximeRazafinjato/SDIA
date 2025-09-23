using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Users;
using SDIA.Core.Organizations;

namespace SDIA.Application.Auth.Register;

public class AuthRegisterValidator : IValidator
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public AuthRegisterValidator(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not AuthRegisterModel registerModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Validate required fields
        validationErrors.AddErrorIfNullOrWhiteSpace(registerModel.FirstName, nameof(registerModel.FirstName), "First name is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(registerModel.LastName, nameof(registerModel.LastName), "Last name is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(registerModel.Email, nameof(registerModel.Email), "Email is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(registerModel.Password, nameof(registerModel.Password), "Password is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(registerModel.ConfirmPassword, nameof(registerModel.ConfirmPassword), "Confirm password is required");

        // Validate field lengths
        validationErrors.AddErrorIfExceedsLength(registerModel.FirstName, 100, nameof(registerModel.FirstName), "First name must not exceed 100 characters");
        validationErrors.AddErrorIfExceedsLength(registerModel.LastName, 100, nameof(registerModel.LastName), "Last name must not exceed 100 characters");
        validationErrors.AddErrorIfExceedsLength(registerModel.Email, 255, nameof(registerModel.Email), "Email must not exceed 255 characters");

        // Validate email format
        validationErrors.AddErrorIfNotEmail(registerModel.Email, nameof(registerModel.Email), "Invalid email format");

        // Validate password
        if (!string.IsNullOrEmpty(registerModel.Password))
        {
            if (registerModel.Password.Length < 8)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(registerModel.Password),
                    ErrorMessage = "Password must be at least 8 characters long"
                });
            }

            if (registerModel.Password != registerModel.ConfirmPassword)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(registerModel.ConfirmPassword),
                    ErrorMessage = "Passwords do not match"
                });
            }
        }

        // Validate phone if provided
        if (!string.IsNullOrEmpty(registerModel.Phone))
        {
            validationErrors.AddErrorIfExceedsLength(registerModel.Phone, 20, nameof(registerModel.Phone), "Phone must not exceed 20 characters");
        }

        // Check if email already exists
        if (!string.IsNullOrWhiteSpace(registerModel.Email))
        {
            var existingUser = await _userRepository.GetByEmailAsync(registerModel.Email, cancellationToken);
            if (existingUser != null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(registerModel.Email),
                    ErrorMessage = "Email already exists"
                });
            }
        }

        // Validate organization if provided
        if (registerModel.OrganizationId.HasValue)
        {
            var organization = await _organizationRepository.GetByIdAsync(registerModel.OrganizationId.Value, cancellationToken);
            if (organization == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(registerModel.OrganizationId),
                    ErrorMessage = "Organization not found"
                });
            }
        }

        // Validate role
        var validRoles = new[] { "Admin", "Manager", "User" };
        if (!validRoles.Contains(registerModel.Role))
        {
            validationErrors.Add(new Ardalis.Result.ValidationError
            {
                Identifier = nameof(registerModel.Role),
                ErrorMessage = "Invalid role specified"
            });
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}