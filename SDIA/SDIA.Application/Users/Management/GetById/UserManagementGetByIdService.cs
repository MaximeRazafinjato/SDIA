using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.GetById;

public class UserManagementGetByIdService
{
    private readonly IUserRepository _userRepository;

    public UserManagementGetByIdService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementGetByIdModel>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserManagementGetByIdModel>.NotFound("User not found");
        }

        var model = MapToModel(user);
        return Result<UserManagementGetByIdModel>.Success(model);
    }

    private static UserManagementGetByIdModel MapToModel(User user)
    {
        return new UserManagementGetByIdModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed,
            LastLoginAt = user.LastLoginAt,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}