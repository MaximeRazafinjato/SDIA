using SDIA.SharedKernel.Models;
using SDIA.SharedKernel.Enums;
using SDIA.Core.Organizations;
using SDIA.Core.FormTemplates;
using SDIA.Core.Users;
using SDIA.Core.Documents;

namespace SDIA.Core.Registrations;

public class Registration : BaseEntity
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Draft;
    
    // Applicant Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsMinor => BirthDate.AddYears(18) > DateTime.UtcNow;
    
    // Registration Details
    public string FormData { get; set; } = string.Empty; // JSON data
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    
    // Authentication
    public string EmailVerificationToken { get; set; } = string.Empty;
    public string SmsVerificationCode { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public bool PhoneVerified { get; set; } = false;
    
    // Relations
    public Guid OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }
    
    public Guid FormTemplateId { get; set; }
    public virtual FormTemplate? FormTemplate { get; set; }
    
    public Guid? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }
    
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<RegistrationHistory> History { get; set; } = new List<RegistrationHistory>();
    public virtual ICollection<RegistrationComment> Comments { get; set; } = new List<RegistrationComment>();
}