using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Upsert;

public class UserManagementUpsertValidator : IValidator
{
    private readonly IUserRepository _userRepository;

    public UserManagementUpsertValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateAsync(UserManagementUpsertModel model, CancellationToken cancellationToken = default, Guid? id = null)
    {
        var validationErrors = new List<ValidationError>();
        var isUpdate = id.HasValue;

        // Check if user exists when updating
        if (isUpdate)
        {
            var entity = await _userRepository.GetByIdAsync(id!.Value, cancellationToken);
            if (entity == null)
            {
                return Result.NotFound("User not found");
            }
        }

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

        // Check email uniqueness
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var existingUser = await _userRepository.GetByEmailAsync(model.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != id)
            {
                validationErrors.Add(new ValidationError { Identifier = nameof(model.Email), ErrorMessage = "A user with this email already exists" });
            }
        }

        // Phone validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Phone, nameof(model.Phone), "Phone is required");
        validationErrors.AddErrorIfNotMatch(model.Phone, @"^\+?[1-9]\d{1,14}$", nameof(model.Phone), "Invalid phone format");

        // Role validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Role, nameof(model.Role), "Role is required");
        if (!string.IsNullOrWhiteSpace(model.Role) && !new[] { "Admin", "Manager", "User" }.Contains(model.Role))
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Role), ErrorMessage = "Role must be Admin, Manager, or User" });
        }

        // Password validation
        if (!isUpdate)
        {
            // Required for new users
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                validationErrors.Add(new ValidationError { Identifier = nameof(model.Password), ErrorMessage = "Password is required for new users" });
            }
            else
            {
                ValidatePassword(model.Password, nameof(model.Password), validationErrors);
            }
        }
        else if (!string.IsNullOrEmpty(model.Password))
        {
            // Optional password validation for updates
            ValidatePassword(model.Password, nameof(model.Password), validationErrors);
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }

    private static void ValidatePassword(string password, string identifier, List<ValidationError> validationErrors)
    {
        if (password.Length < 6)
            validationErrors.Add(new ValidationError { Identifier = identifier, ErrorMessage = "Password must be at least 6 characters long" });

        validationErrors.AddErrorIfNotMatch(
            password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            identifier,
            "Password must contain at least one lowercase letter, one uppercase letter, and one digit");
    }
}