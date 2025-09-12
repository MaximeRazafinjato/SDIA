using Ardalis.Result;
using MediatR;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Delete;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        // Soft delete
        user.IsDeleted = true;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}