namespace SDIA.Application.Auth.ValidateResetToken;

public class ValidateResetTokenDto
{
    public bool IsValid { get; set; }
    public string Email { get; set; } = string.Empty;
}