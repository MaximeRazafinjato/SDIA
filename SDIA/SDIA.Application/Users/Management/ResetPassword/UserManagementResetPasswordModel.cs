namespace SDIA.Application.Users.Management.ResetPassword;

public class UserManagementResetPasswordModel
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
    public bool SendNotification { get; set; } = true;
}

public class UserManagementResetPasswordResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public bool NotificationSent { get; set; }
    public DateTime ResetAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}