using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.Grid;

public class GetUsersGridQuery : GridQuery<UserGridDto>
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public Guid? OrganizationId { get; set; }
}