using Ardalis.Result;
using SDIA.Core.Users;
using System.Security.Cryptography;
using System.Text;

namespace SDIA.Application.Users.Management.ResetPassword;

public class UserManagementResetPasswordService
{
    private readonly IUserRepository _userRepository;

    public UserManagementResetPasswordService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementResetPasswordResult>> ExecuteAsync(
        UserManagementResetPasswordModel model,
        CancellationToken cancellationToken = default)
    {
        // Get the user
        var user = await _userRepository.GetByIdAsync(model.UserId, cancellationToken);
        if (user == null)
        {
            return Result<UserManagementResetPasswordResult>.NotFound("User not found");
        }

        // Hash the new password
        var hashedPassword = HashPassword(model.NewPassword);
        
        // Update user password
        user.PasswordHash = hashedPassword;
        user.UpdatedAt = DateTime.UtcNow;
        
        // Clear any existing refresh tokens for security
        user.RefreshToken = string.Empty;
        user.RefreshTokenExpiry = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // TODO: Send password reset notification email
        
        var result = new UserManagementResetPasswordResult
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TemporaryPassword = model.NewPassword,
            NotificationSent = model.SendNotification, // Would be actual result from email service
            ResetAt = DateTime.UtcNow,
            Success = true,
            Message = "Password reset successfully"
        };

        return Result<UserManagementResetPasswordResult>.Success(result);
    }

    private static string HashPassword(string password)
    {
        // Simple hash for demo - in production use proper password hashing like BCrypt
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}