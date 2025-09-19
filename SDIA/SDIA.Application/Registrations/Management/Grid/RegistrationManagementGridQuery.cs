using SDIA.Application.Common.Models;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Grid;

public class RegistrationManagementGridQuery : GridQuery<RegistrationManagementGridModel>
{
    public RegistrationStatus? Status { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? FormTemplateId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? SubmittedTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? ValidatedFrom { get; set; }
    public DateTime? ValidatedTo { get; set; }
    public DateTime? RejectedFrom { get; set; }
    public DateTime? RejectedTo { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? PhoneVerified { get; set; }
    public bool? IsMinor { get; set; }
    public int? AgeFrom { get; set; }
    public int? AgeTo { get; set; }
}