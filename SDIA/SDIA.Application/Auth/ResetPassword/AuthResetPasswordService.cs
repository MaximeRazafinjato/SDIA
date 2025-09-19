using Ardalis.Result;
using Microsoft.Extensions.Logging;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.ResetPassword;

public class AuthResetPasswordService
{
    private readonly AuthResetPasswordValidator _validator;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthResetPasswordService> _logger;

    public AuthResetPasswordService(
        AuthResetPasswordValidator validator,
        IUserRepository userRepository,
        ILogger<AuthResetPasswordService> logger)
    {
        _validator = validator;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(AuthResetPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var user = await _userRepository.FindSingleAsync(u => u.PasswordResetToken == model.Token, cancellationToken);

        if (user == null)
        {
            return Result.Error("Invalid or expired reset token");
        }

        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            return Result.Error("Reset token has expired");
        }

        // Set new password
        user.SetPassword(model.NewPassword);
        user.PasswordResetToken = string.Empty;
        user.PasswordResetExpiry = null;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password reset successful for user: {Email}", user.Email);

        return Result.Success();
    }
}