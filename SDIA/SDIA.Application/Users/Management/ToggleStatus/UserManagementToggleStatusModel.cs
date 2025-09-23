namespace SDIA.Application.Users.Management.ToggleStatus;

public class UserManagementToggleStatusModel
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}

public class UserManagementToggleStatusResult
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}