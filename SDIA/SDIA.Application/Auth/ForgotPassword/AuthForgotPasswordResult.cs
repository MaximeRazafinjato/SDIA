namespace SDIA.Application.Auth.ForgotPassword;

public class AuthForgotPasswordResult
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}