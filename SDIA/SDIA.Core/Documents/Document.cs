using SDIA.SharedKernel.Models;
using SDIA.Core.Registrations;
using SDIA.Core.Users;

namespace SDIA.Core.Documents;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // ID Card, Certificate, etc.
    public bool IsVerified { get; set; } = false;
    public string VerificationNotes { get; set; } = string.Empty;
    
    // Relations
    public Guid RegistrationId { get; set; }
    public virtual Registration? Registration { get; set; }
    
    public Guid? UploadedByUserId { get; set; }
    public virtual User? UploadedByUser { get; set; }
}