using Ardalis.Result;
using Microsoft.Extensions.Logging;
using SDIA.Core.Users;
using System.Security.Cryptography;

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

    public async Task<Result<AuthForgotPasswordResult>> ExecuteAsync(AuthForgotPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<AuthForgotPasswordResult>.Invalid(validationResult.ValidationErrors);
        }

        var user = await _userRepository.GetByEmailAsync(model.Email, cancellationToken);

        if (user == null)
        {
            // Don't reveal if user exists or not for security
            _logger.LogWarning("Forgot password attempt for non-existent email: {Email}", model.Email);
            return Result<AuthForgotPasswordResult>.Success(new AuthForgotPasswordResult());
        }

        // Generate reset token
        var token = GenerateSecureToken();
        user.PasswordResetToken = token;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user, cancellationToken);

        // In a real application, send email here
        _logger.LogInformation("Password reset token generated for user: {Email}", model.Email);

        var result = new AuthForgotPasswordResult
        {
            Email = user.Email,
            FirstName = user.FirstName,
            Token = token
        };

        return Result<AuthForgotPasswordResult>.Success(result);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}