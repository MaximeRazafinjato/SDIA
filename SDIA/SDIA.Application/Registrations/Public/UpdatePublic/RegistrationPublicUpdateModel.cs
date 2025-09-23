namespace SDIA.Application.Registrations.Public.UpdatePublic;

public class RegistrationPublicUpdateModel
{
    public string Token { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime BirthDate { get; set; }
    public string? FormData { get; set; }
    public bool Submit { get; set; } = false; // If true, changes status to Pending
}
