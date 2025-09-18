using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDIA.API.Data;
using SDIA.API.Models.Registrations;
using SDIA.API.Services;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations-public")]
public class RegistrationsPublicController : ControllerBase
{
    private readonly ILogger<RegistrationsPublicController> _logger;
    private readonly SDIADbContext _context;
    private readonly INotificationLoggerService _notificationLogger;
    private readonly IConfiguration _configuration;

    public RegistrationsPublicController(
        ILogger<RegistrationsPublicController> logger,
        SDIADbContext context,
        INotificationLoggerService notificationLogger,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _notificationLogger = notificationLogger;
        _configuration = configuration;
    }

    /// <summary>
    /// Verify access code and get registration data
    /// </summary>
    [HttpPost("verify-access")]
    public async Task<IActionResult> VerifyAccess([FromBody] VerifyAccessDto dto)
    {
        try
        {
            var registration = await _context.Registrations
                .Include(r => r.Organization)
                .Include(r => r.FormTemplate)
                .FirstOrDefaultAsync(r => r.AccessToken == dto.Token);

            if (registration == null)
            {
                return BadRequest(new { error = "Lien invalide ou expiré" });
            }

            // Check token expiry
            if (registration.AccessTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Ce lien a expiré. Veuillez demander un nouveau lien." });
            }

            // Check verification attempts
            if (registration.VerificationAttempts >= 3)
            {
                return BadRequest(new { error = "Trop de tentatives. Veuillez demander un nouveau lien." });
            }

            // Verify code
            if (registration.SmsVerificationCode != dto.Code)
            {
                registration.VerificationAttempts++;
                await _context.SaveChangesAsync();

                var remainingAttempts = 3 - registration.VerificationAttempts;
                return BadRequest(new {
                    error = $"Code incorrect. {remainingAttempts} tentative(s) restante(s).",
                    remainingAttempts
                });
            }

            // Check code expiry
            if (registration.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Ce code a expiré. Veuillez demander un nouveau code." });
            }

            // Reset attempts on successful verification
            registration.VerificationAttempts = 0;
            await _context.SaveChangesAsync();

            // Generate session token (valid for 4 hours)
            var sessionToken = GenerateSecureToken();
            HttpContext.Session.SetString($"registration_access_{registration.Id}", sessionToken);
            HttpContext.Session.SetString($"registration_token_{dto.Token}", registration.Id.ToString());

            return Ok(new {
                success = true,
                sessionToken,
                registrationId = registration.Id,
                message = "Vérification réussie"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying access");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la vérification" });
        }
    }

    /// <summary>
    /// Get registration data for public access
    /// </summary>
    [HttpGet("{token}")]
    public async Task<IActionResult> GetPublicRegistration(string token, [FromHeader(Name = "X-Session-Token")] string? sessionToken)
    {
        try
        {
            // Verify session
            var registrationIdStr = HttpContext.Session.GetString($"registration_token_{token}");
            if (string.IsNullOrEmpty(registrationIdStr))
            {
                return Unauthorized(new { error = "Session expirée. Veuillez vérifier à nouveau votre code." });
            }

            var registrationId = Guid.Parse(registrationIdStr);
            var storedSessionToken = HttpContext.Session.GetString($"registration_access_{registrationId}");

            if (string.IsNullOrEmpty(storedSessionToken) || storedSessionToken != sessionToken)
            {
                return Unauthorized(new { error = "Session invalide" });
            }

            var registration = await _context.Registrations
                .Include(r => r.Organization)
                .Include(r => r.FormTemplate)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.Author)
                .Include(r => r.Documents)
                .FirstOrDefaultAsync(r => r.Id == registrationId && r.AccessToken == token);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            var dto = MapToPublicDto(registration);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public registration");
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }
    }

    /// <summary>
    /// Update registration data from public access
    /// </summary>
    [HttpPut("{token}")]
    public async Task<IActionResult> UpdatePublicRegistration(
        string token,
        [FromBody] UpdatePublicRegistrationDto dto,
        [FromHeader(Name = "X-Session-Token")] string? sessionToken)
    {
        try
        {
            // Verify session
            var registrationIdStr = HttpContext.Session.GetString($"registration_token_{token}");
            if (string.IsNullOrEmpty(registrationIdStr))
            {
                return Unauthorized(new { error = "Session expirée. Veuillez vérifier à nouveau votre code." });
            }

            var registrationId = Guid.Parse(registrationIdStr);
            var storedSessionToken = HttpContext.Session.GetString($"registration_access_{registrationId}");

            if (string.IsNullOrEmpty(storedSessionToken) || storedSessionToken != sessionToken)
            {
                return Unauthorized(new { error = "Session invalide" });
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.Id == registrationId && r.AccessToken == token);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            // Update fields
            registration.FirstName = dto.FirstName;
            registration.LastName = dto.LastName;
            registration.Email = dto.Email;
            registration.Phone = dto.Phone ?? registration.Phone;
            registration.BirthDate = dto.BirthDate;
            registration.FormData = dto.FormData ?? registration.FormData;
            registration.UpdatedAt = DateTime.UtcNow;

            // If submit is requested and status is Draft, change to Pending
            if (dto.Submit && registration.Status == RegistrationStatus.Draft)
            {
                registration.Status = RegistrationStatus.Pending;
                registration.SubmittedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new {
                success = true,
                message = dto.Submit ? "Dossier soumis avec succès" : "Modifications enregistrées",
                status = registration.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating public registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour" });
        }
    }

    private PublicRegistrationDto MapToPublicDto(Registration registration)
    {
        return new PublicRegistrationDto
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            FormData = registration.FormData,
            CreatedAt = registration.CreatedAt,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason,
            OrganizationName = registration.Organization?.Name ?? "",
            FormTemplateName = registration.FormTemplate?.Name,
            IsMinor = registration.IsMinor,
            Comments = registration.Comments?.Where(c => !c.IsInternal).Select(c => new RegistrationCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorName = c.Author?.FirstName + " " + c.Author?.LastName ?? "Système",
                IsInternal = c.IsInternal
            }).ToList(),
            Documents = registration.Documents?.Select(d => new DocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                DocumentType = d.DocumentType,
                FileSize = d.FileSize,
                UploadedAt = d.CreatedAt
            }).ToList()
        };
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}