using Ardalis.Result;
using Microsoft.Extensions.Logging;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.ForgotPassword;

public class AuthForgotPasswordService
{
    private readonly AuthForgotPasswordValidator _validator;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthForgotPasswordService> _logger;

    public AuthForgotPasswordService(
        AuthForgotPasswordValidator validator,
        IUserRepository userRepository,
        ILogger<AuthForgotPasswordService> logger)
    {
        _validator = validator;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(AuthForgotPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var user = await _userRepository.GetByEmailAsync(model.Email, cancellationToken);

        if (user == null)
        {
            // Don't reveal if user exists or not for security
            _logger.LogWarning("Forgot password attempt for non-existent email: {Email}", model.Email);
            return Result.Success();
        }

        // Generate reset token
        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user, cancellationToken);

        // In a real application, send email here
        _logger.LogInformation("Password reset token generated for user: {Email}", model.Email);

        return Result.Success();
    }
}