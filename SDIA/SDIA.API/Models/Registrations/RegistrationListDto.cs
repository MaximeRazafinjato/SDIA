using SDIA.SharedKernel.Enums;

namespace SDIA.API.Models.Registrations;

public class RegistrationListDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? AssignedToUserName { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public bool IsMinor { get; set; }
}