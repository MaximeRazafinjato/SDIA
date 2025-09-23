namespace SDIA.Application.Registrations.Public.VerifyAccess;

public class RegistrationPublicVerifyAccessResult
{
    public bool Success { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public Guid RegistrationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? RemainingAttempts { get; set; }
}
