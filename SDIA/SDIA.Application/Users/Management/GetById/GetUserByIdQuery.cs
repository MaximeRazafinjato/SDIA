using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.GetById;

public class GetUserByIdQuery : BaseQuery<UserDto>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}