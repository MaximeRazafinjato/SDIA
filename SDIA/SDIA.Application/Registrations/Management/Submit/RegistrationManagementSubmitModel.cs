namespace SDIA.Application.Registrations.Management.Submit;

public class RegistrationManagementSubmitModel
{
    public Guid RegistrationId { get; set; }
    public string? Notes { get; set; }
}

public class RegistrationManagementSubmitResult
{
    public Guid RegistrationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}