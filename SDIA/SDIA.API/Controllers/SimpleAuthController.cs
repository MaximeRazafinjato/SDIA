using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using SDIA.API.Models.Auth;
using SDIA.Core.Services;
using SDIA.Application.Auth.Login;
using SDIA.Application.Auth.GetCurrentUser;
using SDIA.Application.Auth.Register;
using SDIA.Application.Auth.ForgotPassword;
using SDIA.Application.Auth.ResetPassword;
using SDIA.Application.Auth.ValidateResetToken;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/auth")]
public class SimpleAuthController : ControllerBase
{
    private readonly ILogger<SimpleAuthController> _logger;
    private readonly IEmailService _emailService;

    public SimpleAuthController(
        ILogger<SimpleAuthController> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] AuthLoginService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new AuthLoginModel
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { message = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
                    return Unauthorized(new { message = "Invalid email or password" });
                return BadRequest(new { message = result.Errors.FirstOrDefault() });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Value.Id.ToString()),
                new Claim(ClaimTypes.Email, result.Value.Email),
                new Claim(ClaimTypes.Name, $"{result.Value.FirstName} {result.Value.LastName}"),
                new Claim(ClaimTypes.Role, result.Value.Role),
                new Claim("OrganizationId", result.Value.OrganizationId?.ToString() ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _logger.LogInformation("User {Email} logged in successfully", result.Value.Email);

            return Ok(new LoginResponse
            {
                Id = result.Value.Id,
                Email = result.Value.Email,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                Role = result.Value.Role,
                OrganizationId = result.Value.OrganizationId
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
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] AuthGetCurrentUserService service,
        CancellationToken cancellationToken)
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

            var result = await service.ExecuteAsync(userId, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound();
                return BadRequest(new { message = result.Errors.FirstOrDefault() });
            }

            return Ok(new UserResponse
            {
                Id = result.Value.Id,
                Email = result.Value.Email,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                Phone = result.Value.Phone,
                Role = result.Value.Role,
                OrganizationId = result.Value.OrganizationId,
                EmailConfirmed = result.Value.EmailConfirmed,
                PhoneConfirmed = result.Value.PhoneConfirmed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] AuthRegisterService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new AuthRegisterModel
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                OrganizationId = request.OrganizationId
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { message = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.Conflict)
                    return BadRequest(new { message = "Email already exists" });
                return BadRequest(new { message = result.Errors.FirstOrDefault() });
            }

            _logger.LogInformation("New user registered: {Email}", result.Value.Email);

            return Ok(new { message = "Registration successful", userId = result.Value.UserId });
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
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] AuthForgotPasswordService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new AuthForgotPasswordModel
            {
                Email = request.Email
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            // Always return success to prevent email enumeration
            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { message = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
            }
            else if (result.Value != null && !string.IsNullOrEmpty(result.Value.Token))
            {
                // Send reset email
                var resetLink = $"http://localhost:5173/reset-password?token={result.Value.Token}";
                var emailContent = $@"
                    <h2>Réinitialisation de mot de passe</h2>
                    <p>Bonjour {result.Value.FirstName},</p>
                    <p>Vous avez demandé une réinitialisation de votre mot de passe.</p>
                    <p>Cliquez sur le lien ci-dessous pour réinitialiser votre mot de passe :</p>
                    <p><a href='{resetLink}'>Réinitialiser mon mot de passe</a></p>
                    <p>Ce lien expirera dans 1 heure.</p>
                    <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
                    <br>
                    <p>Cordialement,<br>L'équipe SDIA</p>
                ";

                await _emailService.SendEmailAsync(
                    result.Value.Email,
                    "Réinitialisation de votre mot de passe SDIA",
                    emailContent
                );

                _logger.LogInformation("Password reset email sent to {Email}", result.Value.Email);
            }

            return Ok(new { message = "Si l'email existe, un lien de réinitialisation a été envoyé." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for {Email}", request.Email);
            return StatusCode(500, new { message = "Une erreur est survenue. Veuillez réessayer plus tard." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] AuthResetPasswordService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new AuthResetPasswordModel
            {
                Token = request.Token,
                NewPassword = request.NewPassword
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { message = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                {
                    _logger.LogWarning("Invalid reset token used: {Token}", request.Token);
                    return BadRequest(new { message = "Token invalide ou expiré" });
                }
                return BadRequest(new { message = result.Errors.FirstOrDefault() });
            }

            _logger.LogInformation("Password successfully reset for user: {Email}", result.Value.Email);
            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "Une erreur est survenue lors de la réinitialisation" });
        }
    }

    [HttpGet("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken(
        [FromQuery] string token,
        [FromServices] AuthValidateResetTokenService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new AuthValidateResetTokenModel
            {
                Token = token
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                return Ok(new { isValid = false });
            }

            if (!result.Value.IsValid)
            {
                _logger.LogWarning("Invalid or expired reset token validation: {Token}", token);
            }

            return Ok(new
            {
                isValid = result.Value.IsValid,
                email = result.Value.IsValid ? result.Value.Email : string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reset token");
            return StatusCode(500, new { message = "Une erreur est survenue" });
        }
    }

}