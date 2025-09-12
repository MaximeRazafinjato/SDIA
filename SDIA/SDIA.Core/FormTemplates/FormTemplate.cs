using SDIA.SharedKernel.Models;
using SDIA.Core.Organizations;
using SDIA.Core.Registrations;

namespace SDIA.Core.FormTemplates;

public class FormTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsActive { get; set; } = true;
    public string FormSchema { get; set; } = string.Empty; // JSON schema
    
    // Relations
    public Guid OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }
    
    public virtual ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}