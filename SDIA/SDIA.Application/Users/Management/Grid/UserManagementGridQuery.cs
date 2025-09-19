using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.Grid;

public class UserManagementGridQuery : GridQuery<UserManagementGridModel>
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? PhoneConfirmed { get; set; }
    public Guid? OrganizationId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
}