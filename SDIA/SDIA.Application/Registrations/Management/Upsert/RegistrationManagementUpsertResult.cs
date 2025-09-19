namespace SDIA.Application.Registrations.Management.Upsert;

public class RegistrationManagementUpsertResult
{
    public Guid RegistrationId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public bool IsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}