namespace SDIA.Application.Auth.ValidateResetToken;

public class AuthValidateResetTokenModel
{
    public bool IsValid { get; set; }
    public string Email { get; set; } = string.Empty;
}