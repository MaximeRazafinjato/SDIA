namespace SDIA.Application.Auth.Register;

public class AuthRegisterModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "User";
    public Guid? OrganizationId { get; set; }
    public string? Language { get; set; } = "fr";
}

public class AuthRegisterResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool EmailVerificationRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}