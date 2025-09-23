namespace SDIA.Application.Registrations.Public.VerifyCode;

public class RegistrationPublicVerifyCodeModel
{
    public string AccessToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class RegistrationPublicVerifyCodeResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RegistrationId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public int AttemptsRemaining { get; set; }
    public bool MaxAttemptsReached { get; set; }
}