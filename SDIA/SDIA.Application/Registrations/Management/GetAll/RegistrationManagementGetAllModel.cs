namespace SDIA.Application.Registrations.Management.GetAll;

public class RegistrationManagementGetAllModel
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsMinor { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? FormTemplateName { get; set; }
    public string? OrganizationName { get; set; }
    public string? AssignedToUserName { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}