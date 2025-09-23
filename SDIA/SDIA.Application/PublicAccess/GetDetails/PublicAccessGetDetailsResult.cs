namespace SDIA.Application.PublicAccess.GetDetails;

public class PublicAccessGetDetailsResult
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? FormTemplateId { get; set; }
    public string? FormTemplateName { get; set; }
    public string? FormData { get; set; }
    public string? Documents { get; set; }
    public bool CanEdit { get; set; }
}