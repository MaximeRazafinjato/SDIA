using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.ToggleStatus;

public class UserManagementToggleStatusService
{
    private readonly IUserRepository _userRepository;

    public UserManagementToggleStatusService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementToggleStatusResult>> ExecuteAsync(
        UserManagementToggleStatusModel model,
        CancellationToken cancellationToken = default)
    {
        // Get the user
        var user = await _userRepository.GetByIdAsync(model.UserId, cancellationToken);
        if (user == null)
        {
            return Result<UserManagementToggleStatusResult>.NotFound("User not found");
        }

        // Update status
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        var result = new UserManagementToggleStatusResult
        {
            UserId = user.Id,
            IsActive = user.IsActive,
            UserName = $"{user.FirstName} {user.LastName}",
            UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow,
            Success = true,
            Message = model.IsActive ? "User activated successfully" : "User deactivated successfully"
        };

        return Result<UserManagementToggleStatusResult>.Success(result);
    }
}