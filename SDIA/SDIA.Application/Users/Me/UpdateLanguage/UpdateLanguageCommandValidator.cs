using FluentValidation;

namespace SDIA.Application.Users.Me.UpdateLanguage;

public class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    private static readonly string[] SupportedLanguages = { "en", "fr", "es", "de", "it", "pt", "ru", "zh", "ja", "ko" };
    private static readonly string[] CommonTimeZones = {
        "UTC", "America/New_York", "America/Los_Angeles", "America/Chicago", "America/Denver",
        "Europe/London", "Europe/Paris", "Europe/Berlin", "Europe/Rome", "Europe/Madrid",
        "Asia/Tokyo", "Asia/Shanghai", "Asia/Seoul", "Australia/Sydney", "Pacific/Auckland"
    };

    public UpdateLanguageCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
            
        RuleFor(x => x.Language)
            .NotEmpty()
            .WithMessage("Language is required")
            .Must(lang => SupportedLanguages.Contains(lang.ToLower()))
            .WithMessage($"Language must be one of: {string.Join(", ", SupportedLanguages)}");
            
        RuleFor(x => x.TimeZone)
            .Must(tz => string.IsNullOrEmpty(tz) || CommonTimeZones.Contains(tz))
            .WithMessage($"TimeZone must be one of: {string.Join(", ", CommonTimeZones)}")
            .When(x => !string.IsNullOrEmpty(x.TimeZone));
    }
}