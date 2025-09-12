using SDIA.SharedKernel.Models;
using SDIA.SharedKernel.Enums;
using SDIA.Core.Users;

namespace SDIA.Core.Registrations;

public class RegistrationHistory : BaseEntity
{
    public string Action { get; set; } = string.Empty;
    public RegistrationStatus OldStatus { get; set; }
    public RegistrationStatus NewStatus { get; set; }
    public string Details { get; set; } = string.Empty;
    public string ChangedFields { get; set; } = string.Empty; // JSON
    
    // Relations
    public Guid RegistrationId { get; set; }
    public virtual Registration? Registration { get; set; }
    
    public Guid? PerformedByUserId { get; set; }
    public virtual User? PerformedByUser { get; set; }
}