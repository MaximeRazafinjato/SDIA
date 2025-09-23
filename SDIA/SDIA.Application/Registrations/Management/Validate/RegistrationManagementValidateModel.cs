namespace SDIA.Application.Registrations.Management.Validate;

public class RegistrationManagementValidateModel
{
    public Guid RegistrationId { get; set; }
    public bool IsApproved { get; set; } // true for approve, false for reject
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public class RegistrationManagementValidateResult
{
    public Guid RegistrationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}