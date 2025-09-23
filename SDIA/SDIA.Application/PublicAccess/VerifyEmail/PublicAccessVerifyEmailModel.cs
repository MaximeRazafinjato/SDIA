namespace SDIA.Application.PublicAccess.VerifyEmail;

public class PublicAccessVerifyEmailModel
{
    public string Code { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
}

public class PublicAccessVerifyEmailResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}