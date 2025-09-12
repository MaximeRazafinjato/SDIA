using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Management.Upsert;

public class UpsertUserCommand : BaseCommand<UserUpsertResult>
{
    public Guid? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public Guid? OrganizationId { get; set; }

    public bool IsUpdate => Id.HasValue && Id.Value != Guid.Empty;
}