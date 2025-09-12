namespace SDIA.Application.Users.Management.Upsert;

public class UserUpsertResult
{
    public Guid UserId { get; set; }
    public bool IsCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}