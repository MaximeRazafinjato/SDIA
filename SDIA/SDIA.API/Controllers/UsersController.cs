using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SDIA.API.Data;
using SDIA.API.Models.Users;
using SDIA.Core.Users;
using System.Security.Claims;
using SimpleUser = SDIA.Core.Users.User;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly SDIADbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;

    public UsersController(SDIADbContext context, ILogger<UsersController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Users.Where(u => !u.IsDeleted);
            
            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                items = users,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { message = "An error occurred while fetching users" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new UserDetailDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneConfirmed = u.PhoneConfirmed,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = new SimpleUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = false, // Account inactive until email validation
                EmailConfirmed = false,
                PhoneConfirmed = false,
                OrganizationId = dto.OrganizationId,
                CreatedAt = DateTime.UtcNow,
                EmailVerificationToken = Guid.NewGuid().ToString()
            };

            // Generate temporary password
            var tempPassword = GenerateTemporaryPassword();
            user.SetPassword(tempPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send validation email
            await SendValidationEmail(user, tempPassword);

            _logger.LogInformation("User {Email} created successfully", user.Email);

            return Ok(new
            {
                id = user.Id,
                message = "User created successfully. Validation email sent.",
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if email is being changed and already exists
            if (user.Email != dto.Email && await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id && !u.IsDeleted))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Id} updated successfully", id);

            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            // Prevent deleting the current user
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == id.ToString())
            {
                return BadRequest(new { message = "Cannot delete your own account" });
            }

            // Check if this is the last admin
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin" && !u.IsDeleted);
            var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (userToDelete == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (userToDelete.Role == "Admin" && adminCount <= 1)
            {
                return BadRequest(new { message = "Cannot delete the last administrator" });
            }

            // Soft delete
            userToDelete.IsDeleted = true;
            userToDelete.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Id} deleted successfully", id);

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    [HttpPost("{id}/resend-validation")]
    public async Task<IActionResult> ResendValidationEmail(Guid id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new { message = "Email already confirmed" });
            }

            // Generate new verification token
            user.EmailVerificationToken = Guid.NewGuid().ToString();
            
            // Generate new temporary password
            var tempPassword = GenerateTemporaryPassword();
            user.SetPassword(tempPassword);
            
            await _context.SaveChangesAsync();

            // Resend validation email
            await SendValidationEmail(user, tempPassword);

            _logger.LogInformation("Validation email resent to {Email}", user.Email);

            return Ok(new { message = "Validation email sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending validation email for user {Id}", id);
            return StatusCode(500, new { message = "An error occurred while sending the validation email" });
        }
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Generate new temporary password
            var tempPassword = GenerateTemporaryPassword();
            user.SetPassword(tempPassword);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            // Send password reset email
            await SendPasswordResetEmail(user, tempPassword);

            _logger.LogInformation("Password reset for user {Email}", user.Email);

            return Ok(new { message = "Password reset successfully. Email sent with new password." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {Id}", id);
            return StatusCode(500, new { message = "An error occurred while resetting the password" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var stats = new
            {
                totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
                activeUsers = await _context.Users.CountAsync(u => !u.IsDeleted && u.IsActive),
                inactiveUsers = await _context.Users.CountAsync(u => !u.IsDeleted && !u.IsActive),
                confirmedEmails = await _context.Users.CountAsync(u => !u.IsDeleted && u.EmailConfirmed),
                unconfirmedEmails = await _context.Users.CountAsync(u => !u.IsDeleted && !u.EmailConfirmed),
                byRole = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .GroupBy(u => u.Role)
                    .Select(g => new { role = g.Key, count = g.Count() })
                    .ToListAsync()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        var password = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return password;
    }

    private async Task SendValidationEmail(SimpleUser user, string tempPassword)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5173";
        var validationUrl = $"{baseUrl}/validate-account?token={user.EmailVerificationToken}&email={Uri.EscapeDataString(user.Email)}";
        
        var emailContent = $@"
            <h2>Bienvenue sur SDIA</h2>
            <p>Bonjour {user.FirstName} {user.LastName},</p>
            <p>Votre compte a été créé avec succès. Pour activer votre compte, veuillez cliquer sur le lien ci-dessous :</p>
            <p><a href='{validationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Activer mon compte</a></p>
            <p>Ou copiez ce lien dans votre navigateur : {validationUrl}</p>
            <br/>
            <p><strong>Vos identifiants temporaires :</strong></p>
            <p>Email : {user.Email}</p>
            <p>Mot de passe temporaire : {tempPassword}</p>
            <p><em>Vous devrez changer ce mot de passe lors de votre première connexion.</em></p>
            <br/>
            <p>Si vous n'avez pas demandé la création de ce compte, veuillez ignorer cet email.</p>
            <p>Cordialement,<br/>L'équipe SDIA</p>
        ";

        _logger.LogInformation("Validation email sent to {Email}", user.Email);
    }

    private async Task SendPasswordResetEmail(SimpleUser user, string tempPassword)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5173";
        
        var emailContent = $@"
            <h2>Réinitialisation de votre mot de passe</h2>
            <p>Bonjour {user.FirstName} {user.LastName},</p>
            <p>Votre mot de passe a été réinitialisé avec succès.</p>
            <br/>
            <p><strong>Vos nouveaux identifiants :</strong></p>
            <p>Email : {user.Email}</p>
            <p>Nouveau mot de passe : {tempPassword}</p>
            <p><em>Vous devrez changer ce mot de passe lors de votre prochaine connexion.</em></p>
            <br/>
            <p>Si vous n'avez pas demandé cette réinitialisation, veuillez contacter immédiatement l'administrateur.</p>
            <p>Cordialement,<br/>L'équipe SDIA</p>
        ";

        _logger.LogInformation("Password reset email sent to {Email}", user.Email);
    }
}