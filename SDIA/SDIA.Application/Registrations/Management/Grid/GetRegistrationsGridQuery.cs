using SDIA.Application.Common.Models;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Grid;

public class GetRegistrationsGridQuery : GridQuery<RegistrationGridDto>
{
    public RegistrationStatus? Status { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? FormTemplateId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? SubmittedFrom { get; set; }
    public DateTime? SubmittedTo { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? PhoneVerified { get; set; }
}