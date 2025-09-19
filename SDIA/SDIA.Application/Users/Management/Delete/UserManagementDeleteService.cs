using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Delete;

public class UserManagementDeleteService
{
    private readonly IUserRepository _userRepository;

    public UserManagementDeleteService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);

        return Result.Success();
    }
}