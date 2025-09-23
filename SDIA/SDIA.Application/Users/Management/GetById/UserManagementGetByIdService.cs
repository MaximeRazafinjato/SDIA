using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Application.Common.Mappings;
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
        var model = await _userRepository.GetAll(cancellationToken)
            .Where(u => u.Id == id)
            .Select(UserMappings.ToDetailModel())
            .FirstOrDefaultAsync(cancellationToken);

        if (model == null)
        {
            return Result<UserManagementGetByIdModel>.NotFound("User not found");
        }

        return Result<UserManagementGetByIdModel>.Success(model);
    }
}