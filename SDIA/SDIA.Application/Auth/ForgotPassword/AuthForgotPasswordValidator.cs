using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.ForgotPassword;

public class AuthForgotPasswordValidator : IValidator
{
    private readonly IUserRepository _userRepository;

    public AuthForgotPasswordValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateAsync(AuthForgotPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Email validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Email, nameof(model.Email), "Email is required");
        validationErrors.AddErrorIfNotEmail(model.Email, nameof(model.Email), "Invalid email format");

        if (validationErrors.Any())
            return Result.Invalid(validationErrors);

        // Note: Don't reveal if user exists or not for security
        // This check is done in the service for logging purposes only

        return Result.Success();
    }
}