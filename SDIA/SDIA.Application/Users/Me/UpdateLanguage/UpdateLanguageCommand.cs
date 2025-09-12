using SDIA.Application.Common.Models;

namespace SDIA.Application.Users.Me.UpdateLanguage;

public class UpdateLanguageCommand : BaseCommand
{
    public Guid UserId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? TimeZone { get; set; }

    public UpdateLanguageCommand(Guid userId, string language, string? timeZone = null)
    {
        UserId = userId;
        Language = language;
        TimeZone = timeZone;
    }
}