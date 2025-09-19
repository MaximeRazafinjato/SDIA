using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.Grid;

public class UserManagementGridQuery : GridQuery<UserManagementGridModel>
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public Guid? OrganizationId { get; set; }
}