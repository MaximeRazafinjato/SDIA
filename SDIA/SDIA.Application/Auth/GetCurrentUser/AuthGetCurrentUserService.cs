using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.GetCurrentUser;

public class AuthGetCurrentUserService
{
    private readonly IUserRepository _userRepository;

    public AuthGetCurrentUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AuthGetCurrentUserResult>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result<AuthGetCurrentUserResult>.NotFound("User not found");
        }

        var result = new AuthGetCurrentUserResult
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Role = user.Role,
            OrganizationId = user.OrganizationId,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed
        };

        return Result<AuthGetCurrentUserResult>.Success(result);
    }
}