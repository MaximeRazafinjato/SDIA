namespace SDIA.Application.Users.Me.Get;

public class UserMeGetModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string Language { get; set; } = "en";
    public string TimeZone { get; set; } = "UTC";
}