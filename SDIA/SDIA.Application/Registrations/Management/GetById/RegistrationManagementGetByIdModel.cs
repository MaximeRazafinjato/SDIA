using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.GetById;

public class RegistrationManagementGetByIdModel
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsMinor { get; set; }
    public string FormData { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public Guid OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid FormTemplateId { get; set; }
    public string? FormTemplateName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}