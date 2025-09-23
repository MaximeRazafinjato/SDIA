namespace SDIA.Application.PublicAccess.UpdateRegistration;

public class PublicAccessUpdateRegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RegistrationId { get; set; }
}