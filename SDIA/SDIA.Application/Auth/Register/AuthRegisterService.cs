using Ardalis.Result;
using SDIA.Core.Users;
using System.Security.Cryptography;
using System.Text;

namespace SDIA.Application.Auth.Register;

public class AuthRegisterService
{
    private readonly AuthRegisterValidator _validator;
    private readonly IUserRepository _userRepository;

    public AuthRegisterService(
        AuthRegisterValidator validator,
        IUserRepository userRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthRegisterResult>> ExecuteAsync(
        AuthRegisterModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<AuthRegisterResult>.Invalid(validationResult.ValidationErrors);
        }

        // Hash the password
        var hashedPassword = HashPassword(model.Password);

        // Create the user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone ?? string.Empty,
            PasswordHash = hashedPassword,
            Role = model.Role,
            OrganizationId = model.OrganizationId,
            IsActive = true,
            EmailConfirmed = false,
            PhoneConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // TODO: Send email verification
        // TODO: Log registration event

        var result = new AuthRegisterResult
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            EmailVerificationRequired = true,
            CreatedAt = user.CreatedAt,
            Success = true,
            Message = "User registered successfully. Please check your email for verification."
        };

        return Result<AuthRegisterResult>.Success(result);
    }

    private static string HashPassword(string password)
    {
        // Simple hash for demo - in production use proper password hashing like BCrypt
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}