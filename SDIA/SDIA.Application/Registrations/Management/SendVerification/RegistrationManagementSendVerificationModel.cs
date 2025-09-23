namespace SDIA.Application.Registrations.Management.SendVerification;

public class RegistrationManagementSendVerificationModel
{
    public Guid RegistrationId { get; set; }
    public string VerificationType { get; set; } = "email"; // "email" or "sms"
}

public class RegistrationManagementSendVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string VerificationType { get; set; } = string.Empty;
    public string MaskedContact { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public int ExpiresInMinutes { get; set; }
}