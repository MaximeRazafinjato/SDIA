using Ardalis.Result;
using Microsoft.Extensions.Logging;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.ValidateResetToken;

public class AuthValidateResetTokenService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthValidateResetTokenService> _logger;

    public AuthValidateResetTokenService(IUserRepository userRepository, ILogger<AuthValidateResetTokenService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<AuthValidateResetTokenModel>> ExecuteAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Result<AuthValidateResetTokenModel>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Token", ErrorMessage = "Token is required" }
            });
        }

        var user = await _userRepository.FindSingleAsync(u => u.PasswordResetToken == token, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Invalid reset token attempted: {Token}", token);
            return Result<AuthValidateResetTokenModel>.Success(new AuthValidateResetTokenModel
            {
                IsValid = false,
                Email = string.Empty
            });
        }

        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired reset token attempted: {Token}", token);
            return Result<AuthValidateResetTokenModel>.Success(new AuthValidateResetTokenModel
            {
                IsValid = false,
                Email = string.Empty
            });
        }

        return Result<AuthValidateResetTokenModel>.Success(new AuthValidateResetTokenModel
        {
            IsValid = true,
            Email = user.Email
        });
    }
}