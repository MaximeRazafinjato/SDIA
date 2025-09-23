namespace SDIA.Application.Registrations.Management.Assign;

public class RegistrationManagementAssignModel
{
    public Guid RegistrationId { get; set; }
    public Guid? AssignedToUserId { get; set; } // null to unassign
    public string? Notes { get; set; }
}

public class RegistrationManagementAssignResult
{
    public Guid RegistrationId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}