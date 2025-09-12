using SDIA.SharedKernel.Models;
using SDIA.Core.FormTemplates;
using SDIA.Core.Users;
using SDIA.Core.Registrations;

namespace SDIA.Core.Organizations;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // CFA, University, etc.
    public string Logo { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    
    // Configuration
    public string EmailProviderConfig { get; set; } = string.Empty; // JSON config
    public string SmsProviderConfig { get; set; } = string.Empty; // JSON config
    
    // Relations
    public virtual ICollection<FormTemplate> FormTemplates { get; set; } = new List<FormTemplate>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}