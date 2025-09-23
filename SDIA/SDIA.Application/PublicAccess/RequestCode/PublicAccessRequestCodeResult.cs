namespace SDIA.Application.PublicAccess.RequestCode;

public class PublicAccessRequestCodeResult
{
    public string VerificationCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MaskedPhone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; }
}