namespace SDIA.Application.PublicAccess.UpdateRegistration;

public class PublicAccessUpdateRegistrationModel
{
    public Guid Id { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? FormData { get; set; }
}