using Ardalis.Result;
using MediatR;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Me.UpdateLanguage;

public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public UpdateLanguageCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        // Note: The User entity doesn't have Language/TimeZone properties in the current model
        // This would need to be added to the User entity if language preferences are to be persisted
        // For now, this is a placeholder implementation
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        
        return Result.Success();
    }
}