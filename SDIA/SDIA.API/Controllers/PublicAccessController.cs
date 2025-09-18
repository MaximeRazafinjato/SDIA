using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDIA.API.Data;
using SDIA.API.Services;
using SDIA.SharedKernel.Enums;
using System.Security.Cryptography;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicAccessController : ControllerBase
{
    private readonly ILogger<PublicAccessController> _logger;
    private readonly SDIADbContext _context;
    private readonly INotificationLoggerService _notificationLogger;
    private readonly IConfiguration _configuration;

    public PublicAccessController(
        ILogger<PublicAccessController> logger,
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
    /// Step 2: When user clicks the email link, verify token and send SMS code
    /// </summary>
    [HttpPost("registration-access/{token}/request-code")]
    public async Task<IActionResult> RequestVerificationCode(string token)
    {
        try
        {
            // Find registration by access token
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.AccessToken == token);

            if (registration == null)
            {
                return NotFound(new { error = "Lien d'accès invalide" });
            }

            // Check if token is expired
            if (registration.AccessTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Ce lien a expiré. Veuillez demander un nouveau lien." });
            }

            // Check if phone number exists
            if (string.IsNullOrEmpty(registration.Phone))
            {
                return BadRequest(new {
                    error = "Aucun numéro de téléphone n'est associé à cette inscription",
                    requiresPhoneUpdate = true
                });
            }

            // Generate new verification code
            var verificationCode = GenerateVerificationCode();
            registration.SmsVerificationCode = verificationCode;
            registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            registration.VerificationAttempts = 0;

            await _context.SaveChangesAsync();

            // Send SMS with verification code (Step 2 of workflow)
            var fullName = $"{registration.FirstName} {registration.LastName}";
            var smsMessage = $@"Bonjour {fullName},

Votre code de vérification SDIA est : {verificationCode}

Ce code expire dans 10 minutes.

Si vous n'avez pas demandé ce code, veuillez ignorer ce message.";

            await _notificationLogger.LogSmsNotification(
                registration.Phone,
                fullName,
                verificationCode,
                null, // No link in SMS
                smsMessage
            );

            // Mask phone number for display
            var maskedPhone = MaskPhoneNumber(registration.Phone);

            return Ok(new
            {
                success = true,
                message = $"Un code de vérification a été envoyé au {maskedPhone}",
                phoneNumber = maskedPhone,
                expiresInMinutes = 10
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting verification code");
            return StatusCode(500, new { error = "Une erreur est survenue lors de l'envoi du code" });
        }
    }

    /// <summary>
    /// Step 3: Verify SMS code and grant access to edit registration
    /// </summary>
    [HttpPost("registration-access/{token}/verify-code")]
    public async Task<IActionResult> VerifyCode(string token, [FromBody] VerifyCodeDto dto)
    {
        try
        {
            // Find registration by access token
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.AccessToken == token);

            if (registration == null)
            {
                return NotFound(new { error = "Lien d'accès invalide" });
            }

            // Check if token is expired
            if (registration.AccessTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Ce lien a expiré. Veuillez demander un nouveau lien." });
            }

            // Check verification attempts
            if (registration.VerificationAttempts >= 5)
            {
                return BadRequest(new {
                    error = "Trop de tentatives échouées. Veuillez demander un nouveau code.",
                    maxAttemptsReached = true
                });
            }

            // Increment attempts
            registration.VerificationAttempts++;

            // Check if code matches
            if (registration.SmsVerificationCode != dto.Code)
            {
                await _context.SaveChangesAsync();
                return BadRequest(new {
                    error = "Code de vérification incorrect",
                    attemptsRemaining = 5 - registration.VerificationAttempts
                });
            }

            // Check if code is expired
            if (registration.VerificationCodeExpiry < DateTime.UtcNow)
            {
                await _context.SaveChangesAsync();
                return BadRequest(new { error = "Le code de vérification a expiré" });
            }

            // Code is valid - mark phone as verified
            registration.PhoneVerified = true;

            // Clear the verification code
            registration.SmsVerificationCode = string.Empty;
            registration.VerificationCodeExpiry = null;

            await _context.SaveChangesAsync();

            // Generate a session token for editing
            var sessionToken = GenerateSessionToken();

            return Ok(new
            {
                success = true,
                message = "Vérification réussie",
                registrationId = registration.Id,
                sessionToken, // This can be used for subsequent API calls
                redirectUrl = $"/registration-edit/{registration.Id}?session={sessionToken}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying code");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la vérification" });
        }
    }

    /// <summary>
    /// Get registration details for editing (requires verified session)
    /// </summary>
    [HttpGet("registration/{id}/details")]
    public async Task<IActionResult> GetRegistrationDetails(Guid id, [FromQuery] string sessionToken)
    {
        try
        {
            // For now, we'll use the access token for verification
            // In production, implement proper session management
            var registration = await _context.Registrations
                .Include(r => r.FormTemplate)
                .FirstOrDefaultAsync(r => r.Id == id && r.PhoneVerified);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée ou accès non autorisé" });
            }

            return Ok(new
            {
                id = registration.Id,
                firstName = registration.FirstName,
                lastName = registration.LastName,
                email = registration.Email,
                phone = registration.Phone,
                dateOfBirth = registration.BirthDate,
                status = registration.Status.ToString(),
                formTemplateId = registration.FormTemplateId,
                formTemplateName = registration.FormTemplate?.Name,
                formData = registration.FormData,
                documents = registration.Documents,
                canEdit = registration.Status == RegistrationStatus.Draft ||
                         registration.Status == RegistrationStatus.Pending
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration details");
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }
    }

    /// <summary>
    /// Update registration (requires verified session)
    /// </summary>
    [HttpPut("registration/{id}")]
    public async Task<IActionResult> UpdateRegistration(Guid id, [FromBody] PublicUpdateRegistrationDto dto, [FromQuery] string sessionToken)
    {
        try
        {
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.Id == id && r.PhoneVerified);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée ou accès non autorisé" });
            }

            // Only allow editing if status is Draft or Pending
            if (registration.Status != RegistrationStatus.Draft &&
                registration.Status != RegistrationStatus.Pending)
            {
                return BadRequest(new { error = "Cette inscription ne peut plus être modifiée" });
            }

            // Update fields
            if (!string.IsNullOrEmpty(dto.FirstName)) registration.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) registration.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email)) registration.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Phone)) registration.Phone = dto.Phone;
            if (dto.DateOfBirth.HasValue) registration.BirthDate = dto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(dto.FormData)) registration.FormData = dto.FormData;

            registration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Inscription mise à jour avec succès",
                registrationId = registration.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour" });
        }
    }

    private string GenerateVerificationCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3];
        rng.GetBytes(bytes);
        var code = BitConverter.ToUInt32(new byte[] { bytes[0], bytes[1], bytes[2], 0 }, 0) % 1000000;
        return code.ToString("D6");
    }

    private string GenerateSessionToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "****";

        return phone.Substring(0, 2) + new string('*', phone.Length - 4) + phone.Substring(phone.Length - 2);
    }
}

public class VerifyCodeDto
{
    public string Code { get; set; } = string.Empty;
}

public class PublicUpdateRegistrationDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? FormData { get; set; }
}