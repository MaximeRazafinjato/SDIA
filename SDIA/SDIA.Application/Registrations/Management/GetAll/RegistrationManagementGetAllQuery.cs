using SDIA.Application.Common.Models;

namespace SDIA.Application.Registrations.Management.GetAll;

public class RegistrationManagementGetAllQuery : GridQuery
{
    // Status filters
    public string? Status { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? PhoneVerified { get; set; }
    public bool? IsMinor { get; set; }

    // Organization and assignment filters
    public Guid? OrganizationId { get; set; }
    public Guid? FormTemplateId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool? IsAssigned { get; set; }

    // Date range filters
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? SubmittedTo { get; set; }
    public DateTime? ValidatedFrom { get; set; }
    public DateTime? ValidatedTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    // Birth date filters
    public DateTime? BirthDateFrom { get; set; }
    public DateTime? BirthDateTo { get; set; }
}