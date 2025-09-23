using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.ResendValidation;

public class UserManagementResendValidationService
{
    private readonly IUserRepository _userRepository;

    public UserManagementResendValidationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementResendValidationResult>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Result<UserManagementResendValidationResult>.NotFound("User not found");
        }

        if (user.EmailConfirmed)
        {
            return Result<UserManagementResendValidationResult>.Invalid(
                new List<ValidationError>
                {
                    new ValidationError
                    {
                        Identifier = "Email",
                        ErrorMessage = "Email already confirmed"
                    }
                });
        }

        // Generate new verification token
        user.EmailVerificationToken = Guid.NewGuid().ToString();

        // Generate new temporary password
        var tempPassword = GenerateTemporaryPassword();
        user.SetPassword(tempPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserManagementResendValidationResult>.Success(
            new UserManagementResendValidationResult
            {
                Success = true,
                Message = "Validation email sent successfully",
                Email = user.Email,
                TemporaryPassword = tempPassword,
                ValidationToken = user.EmailVerificationToken
            });
    }

    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        var password = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return password;
    }
}

public class UserManagementResendValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public string ValidationToken { get; set; } = string.Empty;
}