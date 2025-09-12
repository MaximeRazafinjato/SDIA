using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Grid;

public class RegistrationGridDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsMinor { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public string? OrganizationName { get; set; }
    public string? FormTemplateName { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}