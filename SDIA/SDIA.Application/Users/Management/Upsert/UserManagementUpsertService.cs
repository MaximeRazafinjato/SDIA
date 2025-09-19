using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Upsert;

public class UserManagementUpsertService
{
    private readonly UserManagementUpsertValidator _validator;
    private readonly IUserRepository _userRepository;

    public UserManagementUpsertService(
        UserManagementUpsertValidator validator,
        IUserRepository userRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementUpsertResult>> ExecuteAsync(UserManagementUpsertModel model, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken, model.Id);
        if (!validationResult.IsSuccess)
        {
            return Result<UserManagementUpsertResult>.Invalid(validationResult.ValidationErrors);
        }

        User user;
        bool isCreated;

        if (model.IsUpdate)
        {
            user = await _userRepository.GetByIdAsync(model.Id!.Value, cancellationToken);
            if (user == null)
            {
                return Result<UserManagementUpsertResult>.NotFound("User not found");
            }
            isCreated = false;
        }
        else
        {
            user = new User();
            isCreated = true;
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.Role = model.Role;
        user.IsActive = model.IsActive;
        user.OrganizationId = model.OrganizationId;

        // Set password if provided (for new users or password change)
        if (!string.IsNullOrEmpty(model.Password))
        {
            user.SetPassword(model.Password);
        }
        else if (isCreated)
        {
            return Result<UserManagementUpsertResult>.Error("Password is required for new users");
        }

        if (isCreated)
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var result = new UserManagementUpsertResult
        {
            UserId = user.Id,
            IsCreated = isCreated,
            Message = isCreated ? "User created successfully" : "User updated successfully"
        };

        return Result<UserManagementUpsertResult>.Success(result);
    }
}