namespace SDIA.Application.Users.Me.UpdateLanguage;

public class UserMeUpdateLanguageModel
{
    public Guid UserId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
}