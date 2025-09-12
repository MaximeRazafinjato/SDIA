using Ardalis.Result;
using MediatR;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Me.Get;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result<CurrentUserDto>.NotFound("User not found");
        }

        var currentUserDto = MapToCurrentUserDto(user);
        return Result<CurrentUserDto>.Success(currentUserDto);
    }

    private static CurrentUserDto MapToCurrentUserDto(User user)
    {
        return new CurrentUserDto
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