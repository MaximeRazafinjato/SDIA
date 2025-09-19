using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Me.Get;

public class UserMeGetService
{
    private readonly IUserRepository _userRepository;

    public UserMeGetService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserMeGetModel>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Result<UserMeGetModel>.NotFound("User not found");
        }

        var model = MapToModel(user);
        return Result<UserMeGetModel>.Success(model);
    }

    private static UserMeGetModel MapToModel(User user)
    {
        return new UserMeGetModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed,
            LastLoginAt = user.LastLoginAt,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization?.Name,
            Language = "en", // Default language
            TimeZone = "UTC" // Default timezone
        };
    }
}