using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Me.UpdateLanguage;

public class UserMeUpdateLanguageService
{
    private readonly IUserRepository _userRepository;

    public UserMeUpdateLanguageService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ExecuteAsync(UserMeUpdateLanguageModel model, CancellationToken cancellationToken = default)
    {
        // Manual validation
        var validationErrors = new List<ValidationError>();

        var supportedLanguages = new[] { "en", "fr", "es", "de", "it", "pt", "ru", "zh", "ja", "ko" };
        var commonTimeZones = new[] {
            "UTC", "America/New_York", "America/Los_Angeles", "America/Chicago", "America/Denver",
            "Europe/London", "Europe/Paris", "Europe/Berlin", "Europe/Rome", "Europe/Madrid",
            "Asia/Tokyo", "Asia/Shanghai", "Asia/Seoul", "Australia/Sydney", "Pacific/Auckland"
        };

        // UserId validation
        if (model.UserId == Guid.Empty)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.UserId), ErrorMessage = "User ID is required" });

        // Language validation
        if (string.IsNullOrWhiteSpace(model.Language))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Language), ErrorMessage = "Language is required" });
        else if (!supportedLanguages.Contains(model.Language.ToLower()))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Language), ErrorMessage = $"Language must be one of: {string.Join(", ", supportedLanguages)}" });

        // TimeZone validation
        if (!string.IsNullOrEmpty(model.TimeZone) && !commonTimeZones.Contains(model.TimeZone))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.TimeZone), ErrorMessage = $"TimeZone must be one of: {string.Join(", ", commonTimeZones)}" });

        if (validationErrors.Any())
            return Result.Invalid(validationErrors);

        var user = await _userRepository.GetByIdAsync(model.UserId, cancellationToken);

        if (user == null)
        {
            return Result.NotFound("User not found");
        }

        // Note: The User entity doesn't have Language/TimeZone properties in the current model
        // This would need to be added to the User entity if language preferences are to be persisted
        // For now, this is a placeholder implementation

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}