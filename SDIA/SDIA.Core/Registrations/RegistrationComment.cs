using SDIA.SharedKernel.Models;
using SDIA.Core.Users;

namespace SDIA.Core.Registrations;

public class RegistrationComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false; // Internal notes not visible to applicants
    
    // Relations
    public Guid RegistrationId { get; set; }
    public virtual Registration? Registration { get; set; }
    
    public Guid AuthorId { get; set; }
    public virtual User? Author { get; set; }
}