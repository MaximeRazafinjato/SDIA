using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Auth.Login;

public class AuthLoginService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthLoginValidator _validator;

    public AuthLoginService(IUserRepository userRepository, AuthLoginValidator validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<AuthLoginResult>> ExecuteAsync(
        AuthLoginModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<AuthLoginResult>.Invalid(validationResult.ValidationErrors);
        }

        var user = await _userRepository.GetByEmailAsync(model.Email, cancellationToken);

        if (user == null || !user.VerifyPassword(model.Password))
        {
            return Result<AuthLoginResult>.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Result<AuthLoginResult>.Error("Account is deactivated");
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var result = new AuthLoginResult
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            OrganizationId = user.OrganizationId
        };

        return Result<AuthLoginResult>.Success(result);
    }
}