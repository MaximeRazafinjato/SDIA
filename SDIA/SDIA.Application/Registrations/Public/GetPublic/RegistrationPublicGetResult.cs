using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Public.GetPublic;

public class RegistrationPublicGetResult
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime BirthDate { get; set; }
    public string? FormData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? FormTemplateName { get; set; }
    public bool IsMinor { get; set; }
    public List<RegistrationPublicCommentResult>? Comments { get; set; }
    public List<RegistrationPublicDocumentResult>? Documents { get; set; }
}

public class RegistrationPublicCommentResult
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class RegistrationPublicDocumentResult
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}
