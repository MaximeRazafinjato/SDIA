using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<AuthValidateResetTokenResult>> ExecuteAsync(AuthValidateResetTokenModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(model.Token))
        {
            return Result<AuthValidateResetTokenResult>.Invalid(new List<ValidationError>
            {
                new ValidationError { Identifier = "Token", ErrorMessage = "Token is required" }
            });
        }

        var user = await _userRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Invalid reset token attempted: {Token}", model.Token);
            return Result<AuthValidateResetTokenResult>.Success(new AuthValidateResetTokenResult
            {
                IsValid = false,
                Email = string.Empty
            });
        }

        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired reset token attempted: {Token}", model.Token);
            return Result<AuthValidateResetTokenResult>.Success(new AuthValidateResetTokenResult
            {
                IsValid = false,
                Email = string.Empty
            });
        }

        return Result<AuthValidateResetTokenResult>.Success(new AuthValidateResetTokenResult
        {
            IsValid = true,
            Email = user.Email
        });
    }
}