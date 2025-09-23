using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<AuthResetPasswordResult>> ExecuteAsync(AuthResetPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<AuthResetPasswordResult>.Invalid(validationResult.ValidationErrors);
        }

        var user = await _userRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token, cancellationToken);

        if (user == null)
        {
            return Result<AuthResetPasswordResult>.Error("Invalid or expired reset token");
        }

        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            return Result<AuthResetPasswordResult>.Error("Reset token has expired");
        }

        // Set new password
        user.SetPassword(model.NewPassword);
        user.PasswordResetToken = string.Empty;
        user.PasswordResetExpiry = null;

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password reset successful for user: {Email}", user.Email);

        var result = new AuthResetPasswordResult
        {
            Email = user.Email,
            Success = true
        };

        return Result<AuthResetPasswordResult>.Success(result);
    }
}