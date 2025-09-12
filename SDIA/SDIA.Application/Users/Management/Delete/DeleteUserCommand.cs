using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.Delete;

public class DeleteUserCommand : BaseCommand
{
    public Guid UserId { get; set; }

    public DeleteUserCommand(Guid userId)
    {
        UserId = userId;
    }
}