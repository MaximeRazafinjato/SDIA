namespace SDIA.Application.Registrations.Public.GetByIdentifier;

public class RegistrationPublicGetByIdentifierModel
{
    public string Identifier { get; set; } = string.Empty; // Could be registration number or access token
}

public class RegistrationPublicGetByIdentifierResult
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FormTemplateName { get; set; }
    public string FormData { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}