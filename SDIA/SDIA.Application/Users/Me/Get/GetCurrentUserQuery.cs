using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Me.Get;

public class GetCurrentUserQuery : BaseQuery<CurrentUserDto>
{
    public Guid UserId { get; set; }

    public GetCurrentUserQuery(Guid userId)
    {
        UserId = userId;
    }
}