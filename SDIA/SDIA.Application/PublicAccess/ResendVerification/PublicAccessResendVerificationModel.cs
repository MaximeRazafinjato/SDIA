namespace SDIA.Application.PublicAccess.ResendVerification;

public class PublicAccessResendVerificationModel
{
    public string Token { get; set; } = string.Empty;
}

public class PublicAccessResendVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int ExpiresInMinutes { get; set; }
    public bool RequiresPhoneUpdate { get; set; }
}