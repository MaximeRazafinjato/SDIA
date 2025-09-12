using SDIA.Application.Common.Models;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Upsert;

public class UpsertRegistrationCommand : BaseCommand<RegistrationUpsertResult>
{
    public Guid? Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Draft;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string FormData { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public Guid FormTemplateId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    public bool IsUpdate => Id.HasValue && Id.Value != Guid.Empty;
}