using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SDIA.Core.Users;
using SDIA.API.Models.Auth;
using SDIA.API.Data;
using SDIA.Core.Services;
using System.Security.Cryptography;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/auth")]
public class SimpleAuthController : ControllerBase
{
    private readonly SDIADbContext _dbContext;
    private readonly ILogger<SimpleAuthController> _logger;
    private readonly IEmailService _emailService;

    public SimpleAuthController(
        SDIADbContext dbContext, 
        ILogger<SimpleAuthController> logger,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null || !user.VerifyPassword(request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("OrganizationId", user.OrganizationId?.ToString() ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                OrganizationId = user.OrganizationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Unauthorized();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Role = user.Role,
                OrganizationId = user.OrganizationId,
                EmailConfirmed = user.EmailConfirmed,
                PhoneConfirmed = user.PhoneConfirmed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            {
                return BadRequest(new { message = "First name and last name are required" });
            }

            // Check if user already exists
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone ?? string.Empty,
                Role = "User",
                IsActive = true,
                OrganizationId = request.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };

            user.SetPassword(request.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", user.Email);

            return Ok(new { message = "Registration successful", userId = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "Auth controller is working", timestamp = DateTime.UtcNow });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "L'email est requis" });
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            // Always return success to prevent email enumeration
            if (user == null)
            {
                _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
                return Ok(new { message = "Si l'email existe, un lien de réinitialisation a été envoyé." });
            }

            // Generate secure reset token
            var token = GenerateSecureToken();
            user.PasswordResetToken = token;
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);

            await _dbContext.SaveChangesAsync();

            // Send reset email
            var resetLink = $"http://localhost:5173/reset-password?token={token}";
            var emailContent = $@"
                <h2>Réinitialisation de mot de passe</h2>
                <p>Bonjour {user.FirstName},</p>
                <p>Vous avez demandé une réinitialisation de votre mot de passe.</p>
                <p>Cliquez sur le lien ci-dessous pour réinitialiser votre mot de passe :</p>
                <p><a href='{resetLink}'>Réinitialiser mon mot de passe</a></p>
                <p>Ce lien expirera dans 1 heure.</p>
                <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
                <br>
                <p>Cordialement,<br>L'équipe SDIA</p>
            ";

            await _emailService.SendEmailAsync(
                user.Email,
                "Réinitialisation de votre mot de passe SDIA",
                emailContent
            );

            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            return Ok(new { message = "Si l'email existe, un lien de réinitialisation a été envoyé." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for {Email}", request.Email);
            return StatusCode(500, new { message = "Une erreur est survenue. Veuillez réessayer plus tard." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Token et nouveau mot de passe requis" });
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null)
            {
                _logger.LogWarning("Invalid reset token used: {Token}", request.Token);
                return BadRequest(new { message = "Token invalide ou expiré" });
            }

            if (user.PasswordResetExpiry == null || user.PasswordResetExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired reset token used for user: {Email}", user.Email);
                return BadRequest(new { message = "Token invalide ou expiré" });
            }

            // Reset password
            user.SetPassword(request.NewPassword);
            user.PasswordResetToken = string.Empty;
            user.PasswordResetExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Password successfully reset for user: {Email}", user.Email);
            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "Une erreur est survenue lors de la réinitialisation" });
        }
    }

    [HttpGet("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Ok(new { isValid = false });
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);

            if (user == null)
            {
                _logger.LogWarning("Invalid reset token validation attempt: {Token}", token);
                return Ok(new { isValid = false });
            }

            var isValid = user.PasswordResetExpiry != null && user.PasswordResetExpiry > DateTime.UtcNow;

            if (!isValid)
            {
                _logger.LogWarning("Expired reset token validation for user: {Email}", user.Email);
            }

            return Ok(new 
            { 
                isValid = isValid,
                email = isValid ? user.Email : string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reset token");
            return StatusCode(500, new { message = "Une erreur est survenue" });
        }
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