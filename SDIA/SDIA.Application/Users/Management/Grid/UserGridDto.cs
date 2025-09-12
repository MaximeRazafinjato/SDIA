namespace SDIA.Application.Users.Management.Grid;

public class UserGridDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? OrganizationName { get; set; }
    public DateTime CreatedAt { get; set; }
}