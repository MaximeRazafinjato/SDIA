using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SDIA.Core.Users;
using SDIA.API.Models.Auth;
using SDIA.API.Data;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/auth")]
public class SimpleAuthController : ControllerBase
{
    private readonly SimpleSDIADbContext _dbContext;
    private readonly ILogger<SimpleAuthController> _logger;

    public SimpleAuthController(SimpleSDIADbContext dbContext, ILogger<SimpleAuthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
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
}