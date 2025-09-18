namespace SDIA.API.Models.Registrations;

public class GenerateAccessLinkDto
{
    public bool SendNotification { get; set; } = true;
    public string? CustomMessage { get; set; }
}