namespace SDIA.Application.Registrations.Public.Submit;

public class RegistrationPublicSubmitModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Guid FormTemplateId { get; set; }
    public Guid OrganizationId { get; set; }
    public string FormData { get; set; } = "{}"; // JSON data
}

public class RegistrationPublicSubmitResult
{
    public Guid RegistrationId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
}