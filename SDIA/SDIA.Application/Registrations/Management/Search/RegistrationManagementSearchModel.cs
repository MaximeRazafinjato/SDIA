namespace SDIA.Application.Registrations.Management.Search;

public class RegistrationManagementSearchModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 50;
    public Guid? OrganizationId { get; set; }
    public string? Status { get; set; }
}

public class RegistrationManagementSearchResult
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FormTemplateName { get; set; }
    public DateTime CreatedAt { get; set; }
}