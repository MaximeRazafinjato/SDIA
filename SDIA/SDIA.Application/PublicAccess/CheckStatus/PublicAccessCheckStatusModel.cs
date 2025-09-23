namespace SDIA.Application.PublicAccess.CheckStatus;

public class PublicAccessCheckStatusModel
{
    public Guid RegistrationId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
}

public class PublicAccessCheckStatusResult
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid FormTemplateId { get; set; }
    public string? FormTemplateName { get; set; }
    public string FormData { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}