using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SDIA.Infrastructure.Data;
using SDIA.API.Models.Registrations;
using SDIA.API.Services;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using SDIA.Application.Registrations.Management.Grid;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly ILogger<RegistrationsController> _logger;
    private readonly SDIADbContext _context;
    private readonly INotificationLoggerService _notificationLogger;
    private readonly IConfiguration _configuration;

    public RegistrationsController(
        ILogger<RegistrationsController> logger,
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
    /// Get registrations grid (POST)
    /// </summary>
    [HttpPost("grid")]
    [Authorize]
    public async Task<IActionResult> GetGrid(
        [FromBody] RegistrationManagementGridQuery query,
        [FromServices] RegistrationManagementGridService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get registration statistics
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var allRegistrations = await _context.Registrations.ToListAsync();
            var stats = allRegistrations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var total = stats.Sum(s => s.Count);
            var pendingCount = stats.FirstOrDefault(s => s.Status == RegistrationStatus.Pending)?.Count ?? 0;
            var validatedCount = stats.FirstOrDefault(s => s.Status == RegistrationStatus.Validated)?.Count ?? 0;
            var rejectedCount = stats.FirstOrDefault(s => s.Status == RegistrationStatus.Rejected)?.Count ?? 0;
            var draftCount = stats.FirstOrDefault(s => s.Status == RegistrationStatus.Draft)?.Count ?? 0;

            return Ok(new
            {
                total,
                pending = pendingCount,
                validated = validatedCount,
                rejected = rejectedCount,
                byStatus = new[]
                {
                    new { status = "Pending", count = pendingCount },
                    new { status = "Draft", count = draftCount },
                    new { status = "Validated", count = validatedCount },
                    new { status = "Rejected", count = rejectedCount }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration statistics");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération des statistiques" });
        }
    }

    /// <summary>
    /// Get list of registrations
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(
        [FromQuery] RegistrationStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Registrations
                .Include(r => r.Organization)
                .Include(r => r.AssignedToUser)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => 
                    r.FirstName.Contains(searchTerm) ||
                    r.LastName.Contains(searchTerm) ||
                    r.Email.Contains(searchTerm) ||
                    r.RegistrationNumber.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.RegistrationNumber,
                    r.FirstName,
                    r.LastName,
                    r.Email,
                    r.Phone,
                    r.Status,
                    r.CreatedAt,
                    r.SubmittedAt,
                    OrganizationName = r.Organization != null ? r.Organization.Name : null,
                    r.IsMinor,
                    AssignedToUserName = r.AssignedToUser != null ? $"{r.AssignedToUser.FirstName} {r.AssignedToUser.LastName}" : null
                })
                .ToListAsync();

            var result = new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registrations");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération des inscriptions" });
        }
    }

    /// <summary>
    /// Get registration by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRegistration(Guid id)
    {
        try
        {
            var registration = await _context.Registrations
                .Include(r => r.Organization)
                .Include(r => r.AssignedToUser)
                .Include(r => r.FormTemplate)
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            return Ok(registration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration {RegistrationId}", id);
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération de l'inscription" });
        }
    }

    /// <summary>
    /// Create a new registration
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var registration = new Registration
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = $"REG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Status = RegistrationStatus.Draft,
                OrganizationId = user.OrganizationId ?? Guid.Empty,
                FormTemplateId = dto.FormTemplateId,
                FormData = dto.FormData,
                BirthDate = dto.BirthDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistration), new { id = registration.Id }, registration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la création de l'inscription" });
        }
    }

    /// <summary>
    /// Update registration
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRegistration(Guid id, [FromBody] UpdateRegistrationDto dto)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            // Update fields
            registration.FirstName = dto.FirstName ?? registration.FirstName;
            registration.LastName = dto.LastName ?? registration.LastName;
            registration.Email = dto.Email ?? registration.Email;
            registration.Phone = dto.Phone ?? registration.Phone;
            registration.BirthDate = dto.BirthDate ?? registration.BirthDate;
            registration.FormData = dto.FormData ?? registration.FormData;
            registration.UpdatedAt = DateTime.UtcNow;

            // Update status if provided
            if (dto.Status.HasValue)
            {
                registration.Status = dto.Status.Value;

                if (dto.Status == RegistrationStatus.Validated && !registration.ValidatedAt.HasValue)
                {
                    registration.ValidatedAt = DateTime.UtcNow;
                }
                else if (dto.Status == RegistrationStatus.Rejected && !registration.RejectedAt.HasValue)
                {
                    registration.RejectedAt = DateTime.UtcNow;
                    registration.RejectionReason = dto.RejectionReason ?? "Dossier non conforme";
                }
                else if (dto.Status == RegistrationStatus.Pending && !registration.SubmittedAt.HasValue)
                {
                    registration.SubmittedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Inscription mise à jour avec succès", registration });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour de l'inscription" });
        }
    }

    /// <summary>
    /// Update registration status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            registration.Status = dto.Status;
            registration.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == RegistrationStatus.Pending && !registration.SubmittedAt.HasValue)
            {
                registration.SubmittedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Statut mis à jour avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration status");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour du statut" });
        }
    }

    /// <summary>
    /// Perform action on registration
    /// </summary>
    [HttpPost("{id}/action")]
    [Authorize]
    public async Task<IActionResult> PerformAction(Guid id, [FromBody] RegistrationActionDto dto)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            var action = dto.ActionType.ToLower();
            switch (action)
            {
                case "validate":
                    registration.Status = RegistrationStatus.Validated;
                    break;
                case "reject":
                    registration.Status = RegistrationStatus.Rejected;
                    break;
                case "submit":
                    registration.Status = RegistrationStatus.Pending;
                    registration.SubmittedAt = DateTime.UtcNow;
                    break;
                default:
                    return BadRequest(new { error = "Action non reconnue" });
            }

            registration.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Action '{action}' effectuée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing action on registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de l'exécution de l'action" });
        }
    }

    /// <summary>
    /// Generate public access link for a registration
    /// </summary>
    [HttpPost("{id}/generate-access-link")]
    [Authorize]
    public async Task<IActionResult> GenerateAccessLink(Guid id, [FromBody] GenerateAccessLinkDto? dto)
    {
        try
        {
            var registration = await _context.Registrations
                .Include(r => r.Organization)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            // Generate access token and verification code
            var accessToken = GenerateSecureToken();
            var verificationCode = GenerateVerificationCode();

            // Update registration
            registration.AccessToken = accessToken;
            registration.AccessTokenExpiry = DateTime.UtcNow.AddHours(24);
            registration.SmsVerificationCode = verificationCode;
            registration.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            registration.VerificationAttempts = 0;
            registration.LastReminderSentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Generate link
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var accessLink = $"{baseUrl}/registration-access/{accessToken}";

            // Log notification
            var fullName = $"{registration.FirstName} {registration.LastName}";
            var message = dto?.CustomMessage ?? GenerateDefaultMessage(fullName, verificationCode, accessLink);

            if (dto?.SendNotification ?? true)
            {
                if (!string.IsNullOrEmpty(registration.Phone))
                {
                    await _notificationLogger.LogSmsNotification(
                        registration.Phone,
                        fullName,
                        verificationCode,
                        accessLink,
                        message
                    );
                }
                else
                {
                    await _notificationLogger.LogEmailNotification(
                        registration.Email,
                        fullName,
                        verificationCode,
                        accessLink,
                        message
                    );
                }
            }

            return Ok(new
            {
                success = true,
                accessLink,
                verificationCode,
                expiresAt = registration.AccessTokenExpiry,
                notificationType = !string.IsNullOrEmpty(registration.Phone) ? "SMS" : "Email",
                recipient = !string.IsNullOrEmpty(registration.Phone) ? registration.Phone : registration.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access link");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la génération du lien" });
        }
    }

    /// <summary>
    /// Send reminder with access link
    /// </summary>
    [HttpPost("{id}/send-reminder")]
    [Authorize]
    public async Task<IActionResult> SendReminder(Guid id, [FromBody] GenerateAccessLinkDto? dto)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            // Generate new access token for the email link
            var token = GenerateSecureToken();
            registration.AccessToken = token;
            registration.AccessTokenExpiry = DateTime.UtcNow.AddDays(7);
            registration.LastReminderSentAt = DateTime.UtcNow;

            // Clear any existing verification code (will be generated when link is clicked)
            registration.SmsVerificationCode = string.Empty;
            registration.VerificationCodeExpiry = null;
            registration.VerificationAttempts = 0;

            await _context.SaveChangesAsync();

            // Generate link
            var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
            var accessLink = $"{baseUrl}/registration-access/{token}";

            // Send EMAIL only (Step 1 of workflow)
            var fullName = $"{registration.FirstName} {registration.LastName}";
            var emailMessage = dto?.CustomMessage ?? $@"Bonjour {fullName},

Vous avez une inscription en cours qui nécessite votre attention.

Pour accéder à votre dossier, veuillez cliquer sur le lien suivant :
{accessLink}

Ce lien est valable pendant 7 jours.

Une fois que vous cliquerez sur le lien, un code de vérification sera envoyé par SMS sur votre téléphone pour sécuriser l'accès.

Cordialement,
L'équipe SDIA";

            await _notificationLogger.LogEmailNotification(
                registration.Email,
                fullName,
                null, // No verification code in email
                accessLink,
                emailMessage
            );

            return Ok(new
            {
                success = true,
                message = "Email de rappel envoyé avec succès",
                accessLink,
                expiresAt = registration.AccessTokenExpiry,
                notificationType = "Email",
                recipient = registration.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reminder");
            return StatusCode(500, new { error = "Une erreur est survenue lors de l'envoi du rappel" });
        }
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string GenerateDefaultMessage(string name, string code, string link)
    {
        return $@"Bonjour {name},

Vous avez demandé l'accès à votre dossier d'inscription.

Votre code de vérification est : {code}

Cliquez sur le lien suivant pour accéder à votre dossier :
{link}

Ce lien est valable 24 heures et le code expire dans 10 minutes.

Cordialement,
L'équipe SDIA";
    }

    private string GenerateReminderMessage(string name, string code, string link)
    {
        return $@"Bonjour {name},

Rappel : Votre dossier d'inscription est en attente de complétion.

Votre nouveau code de vérification est : {code}

Cliquez sur le lien suivant pour accéder à votre dossier :
{link}

Ce code expire dans 10 minutes.

Cordialement,
L'équipe SDIA";
    }

    /// <summary>
    /// Validate a registration
    /// </summary>
    [HttpPost("{id}/validate")]
    [Authorize]
    public async Task<IActionResult> ValidateRegistration(Guid id, [FromBody] ValidateRejectDto dto)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            registration.Status = RegistrationStatus.Validated;
            registration.ValidatedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;

            // Add comment if provided
            if (!string.IsNullOrEmpty(dto?.Comments))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(Guid.Parse(userId!));

                var comment = new RegistrationComment
                {
                    Id = Guid.NewGuid(),
                    Content = dto.Comments,
                    RegistrationId = id,
                    AuthorId = Guid.Parse(userId!),
                    IsInternal = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RegistrationComments.Add(comment);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Inscription validée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating registration");
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }
    }

    /// <summary>
    /// Reject a registration
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectRegistration(Guid id, [FromBody] ValidateRejectDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto?.Reason))
            {
                return BadRequest(new { error = "La raison du rejet est obligatoire" });
            }

            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }

            registration.Status = RegistrationStatus.Rejected;
            registration.RejectedAt = DateTime.UtcNow;
            registration.RejectionReason = dto.Reason;
            registration.UpdatedAt = DateTime.UtcNow;

            // Add rejection reason as comment
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var comment = new RegistrationComment
            {
                Id = Guid.NewGuid(),
                Content = $"Dossier rejeté : {dto.Reason}",
                RegistrationId = id,
                AuthorId = Guid.Parse(userId!),
                IsInternal = false, // Visible to applicant
                CreatedAt = DateTime.UtcNow
            };

            _context.RegistrationComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Inscription rejetée" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting registration");
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }
    }

    /// <summary>
    /// Send reminder to complete registration (alias for send-reminder)
    /// </summary>
    [HttpPost("{id}/remind")]
    [Authorize]
    public async Task<IActionResult> Remind(Guid id, [FromBody] GenerateAccessLinkDto? dto)
    {
        // Simply call the existing SendReminder method
        return await SendReminder(id, dto);
    }
}

public class ValidateRejectDto
{
    public string? Comments { get; set; }
    public string? Reason { get; set; }
}