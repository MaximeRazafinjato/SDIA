using SDIA.SharedKernel.Models;
using SDIA.Core.Organizations;
using SDIA.Core.Registrations;
using System.Security.Cryptography;
using System.Text;

namespace SDIA.Core.Users;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, Manager, User
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public bool PhoneConfirmed { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiry { get; set; }
    public string EmailVerificationToken { get; set; } = string.Empty;
    
    // Relations
    public Guid? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }
    public virtual ICollection<Registration> ManagedRegistrations { get; set; } = new List<Registration>();
    
    // Methods
    public void SetPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + "SDIA_SALT_2024"; // Simple salt for demo
        PasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword)));
    }
    
    public bool VerifyPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + "SDIA_SALT_2024"; // Same salt for demo
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword)));
        return computedHash == PasswordHash;
    }
}