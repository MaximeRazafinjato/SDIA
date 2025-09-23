using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDIA.API.Models.Registrations;
using SDIA.API.Services;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using SDIA.Application.Registrations.Management.Grid;
using SDIA.Application.Registrations.Management.GetById;
using SDIA.Application.Registrations.Management.Statistics;
using SDIA.Application.Registrations.Management.GetAll;
using SDIA.Application.Registrations.Management.Create;
using SDIA.Application.Registrations.Management.Update;
using SDIA.Application.Registrations.Management.UpdateStatus;
using SDIA.Application.Registrations.Management.Action;
using SDIA.Application.Registrations.Management.GenerateAccessLink;
using SDIA.Application.Registrations.Management.SendReminder;
using SDIA.Application.Registrations.Management.Validate;
using SDIA.Application.Registrations.Management.Reject;
using System.Security.Claims;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly ILogger<RegistrationsController> _logger;
    private readonly INotificationLoggerService _notificationLogger;
    private readonly IConfiguration _configuration;

    public RegistrationsController(
        ILogger<RegistrationsController> logger,
        INotificationLoggerService notificationLogger,
        IConfiguration configuration)
    {
        _logger = logger;
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
    public async Task<IActionResult> GetStatistics(
        [FromServices] RegistrationManagementStatisticsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("Error getting registration statistics: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération des statistiques" });
        }

        var stats = result.Value;
        return Ok(new
        {
            total = stats.TotalRegistrations,
            pending = stats.PendingCount,
            validated = stats.ValidatedCount,
            rejected = stats.RejectedCount,
            byStatus = new[]
            {
                new { status = "Pending", count = stats.PendingCount },
                new { status = "Draft", count = stats.DraftCount },
                new { status = "Validated", count = stats.ValidatedCount },
                new { status = "Rejected", count = stats.RejectedCount }
            }
        });
    }

    /// <summary>
    /// Get list of registrations
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(
        [FromQuery] RegistrationStatus? status,
        [FromQuery] string? searchTerm,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] RegistrationManagementGetAllService service,
        CancellationToken cancellationToken)
    {
        var query = new RegistrationManagementGetAllQuery
        {
            Status = status?.ToString(),
            SearchTerm = searchTerm,
            Page = page == 0 ? 1 : page,
            PageSize = pageSize == 0 ? 20 : pageSize
        };

        var result = await service.ExecuteAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogError("Error getting registrations: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération des inscriptions" });
        }

        var gridResult = result.Value;
        return Ok(new
        {
            items = gridResult.Data,
            totalCount = gridResult.TotalCount,
            page = gridResult.Page,
            pageSize = gridResult.PageSize,
            totalPages = (int)Math.Ceiling(gridResult.TotalCount / (double)gridResult.PageSize)
        });
    }

    /// <summary>
    /// Get registration by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRegistration(
        Guid id,
        [FromServices] RegistrationManagementGetByIdService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error getting registration {RegistrationId}: {Errors}", id, string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération de l'inscription" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new registration
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRegistration(
        [FromBody] CreateRegistrationDto dto,
        [FromServices] RegistrationManagementCreateService service,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var model = new RegistrationManagementCreateModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            FormTemplateId = dto.FormTemplateId,
            FormData = dto.FormData,
            BirthDate = dto.BirthDate
        };

        var result = await service.ExecuteAsync(model, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.Unauthorized)
            {
                return Unauthorized();
            }
            _logger.LogError("Error creating registration: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la création de l'inscription" });
        }

        return CreatedAtAction(nameof(GetRegistration), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Update registration
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateRegistration(
        Guid id,
        [FromBody] UpdateRegistrationDto dto,
        [FromServices] RegistrationManagementUpdateService service,
        CancellationToken cancellationToken)
    {
        var model = new RegistrationManagementUpdateModel
        {
            Id = id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            BirthDate = dto.BirthDate,
            FormData = dto.FormData,
            Status = dto.Status,
            RejectionReason = dto.RejectionReason
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error updating registration: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour de l'inscription" });
        }

        return Ok(new { message = result.Value.Message, registration = result.Value });
    }

    /// <summary>
    /// Update registration status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusDto dto,
        [FromServices] RegistrationManagementUpdateStatusService service,
        CancellationToken cancellationToken)
    {
        var model = new RegistrationManagementUpdateStatusModel
        {
            Id = id,
            Status = dto.Status
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error updating registration status: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour du statut" });
        }

        return Ok(new { message = "Statut mis à jour avec succès" });
    }

    /// <summary>
    /// Perform action on registration
    /// </summary>
    [HttpPost("{id}/action")]
    [Authorize]
    public async Task<IActionResult> PerformAction(
        Guid id,
        [FromBody] RegistrationActionDto dto,
        [FromServices] RegistrationManagementActionService service,
        CancellationToken cancellationToken)
    {
        var model = new RegistrationManagementActionModel
        {
            Id = id,
            ActionType = dto.ActionType
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "Action non reconnue" });
            }
            _logger.LogError("Error performing action on registration: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de l'exécution de l'action" });
        }

        return Ok(new { message = $"Action '{dto.ActionType}' effectuée avec succès" });
    }

    /// <summary>
    /// Generate public access link for a registration
    /// </summary>
    [HttpPost("{id}/generate-access-link")]
    [Authorize]
    public async Task<IActionResult> GenerateAccessLink(
        Guid id,
        [FromBody] GenerateAccessLinkDto? dto,
        [FromServices] RegistrationManagementGenerateAccessLinkService service,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";

        var model = new RegistrationManagementGenerateAccessLinkModel
        {
            RegistrationId = id,
            BaseUrl = baseUrl,
            SendNotification = dto?.SendNotification ?? true,
            CustomMessage = dto?.CustomMessage
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error generating access link: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de la génération du lien" });
        }

        // Log notification if requested
        if (model.SendNotification)
        {
            var message = dto?.CustomMessage ?? GenerateDefaultMessage(
                result.Value.RegistrationName,
                result.Value.VerificationCode,
                result.Value.AccessLink);

            if (result.Value.NotificationType == "SMS")
            {
                await _notificationLogger.LogSmsNotification(
                    result.Value.Recipient,
                    result.Value.RegistrationName,
                    result.Value.VerificationCode,
                    result.Value.AccessLink,
                    message
                );
            }
            else
            {
                await _notificationLogger.LogEmailNotification(
                    result.Value.Recipient,
                    result.Value.RegistrationName,
                    result.Value.VerificationCode,
                    result.Value.AccessLink,
                    message
                );
            }
        }

        return Ok(new
        {
            success = result.Value.Success,
            accessLink = result.Value.AccessLink,
            verificationCode = result.Value.VerificationCode,
            expiresAt = result.Value.ExpiresAt,
            notificationType = result.Value.NotificationType,
            recipient = result.Value.Recipient
        });
    }

    /// <summary>
    /// Send reminder with access link
    /// </summary>
    [HttpPost("{id}/send-reminder")]
    [Authorize]
    public async Task<IActionResult> SendReminder(
        Guid id,
        [FromBody] GenerateAccessLinkDto? dto,
        [FromServices] RegistrationManagementSendReminderService service,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";

        var model = new RegistrationManagementSendReminderModel
        {
            RegistrationId = id,
            BaseUrl = baseUrl,
            CustomMessage = dto?.CustomMessage
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error sending reminder: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue lors de l'envoi du rappel" });
        }

        // Send EMAIL only (Step 1 of workflow)
        var emailMessage = dto?.CustomMessage ?? $@"Bonjour {result.Value.RegistrationName},

Vous avez une inscription en cours qui nécessite votre attention.

Pour accéder à votre dossier, veuillez cliquer sur le lien suivant :
{result.Value.AccessLink}

Ce lien est valable pendant 7 jours.

Une fois que vous cliquerez sur le lien, un code de vérification sera envoyé par SMS sur votre téléphone pour sécuriser l'accès.

Cordialement,
L'équipe SDIA";

        await _notificationLogger.LogEmailNotification(
            result.Value.Recipient,
            result.Value.RegistrationName,
            null, // No verification code in email
            result.Value.AccessLink,
            emailMessage
        );

        return Ok(new
        {
            success = result.Value.Success,
            message = result.Value.Message,
            accessLink = result.Value.AccessLink,
            expiresAt = result.Value.ExpiresAt,
            notificationType = result.Value.NotificationType,
            recipient = result.Value.Recipient
        });
    }

    /// <summary>
    /// Validate a registration
    /// </summary>
    [HttpPost("{id}/validate")]
    [Authorize]
    public async Task<IActionResult> ValidateRegistration(
        Guid id,
        [FromBody] ValidateRejectDto dto,
        [FromServices] RegistrationManagementValidateService service,
        CancellationToken cancellationToken)
    {
        var model = new RegistrationManagementValidateModel
        {
            RegistrationId = id,
            IsApproved = true,
            Notes = dto?.Comments
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            _logger.LogError("Error validating registration: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }

        return Ok(new { success = true, message = "Inscription validée avec succès" });
    }

    /// <summary>
    /// Reject a registration
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectRegistration(
        Guid id,
        [FromBody] ValidateRejectDto dto,
        [FromServices] RegistrationManagementRejectService service,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var model = new RegistrationManagementRejectModel
        {
            RegistrationId = id,
            Reason = dto?.Reason ?? string.Empty,
            Comments = dto?.Comments
        };

        var result = await service.ExecuteAsync(model, userId, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                return NotFound(new { error = "Inscription non trouvée" });
            }
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            {
                return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "La raison du rejet est obligatoire" });
            }
            _logger.LogError("Error rejecting registration: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(500, new { error = "Une erreur est survenue" });
        }

        return Ok(new { success = true, message = "Inscription rejetée" });
    }

    /// <summary>
    /// Send reminder to complete registration (alias for send-reminder)
    /// </summary>
    [HttpPost("{id}/remind")]
    [Authorize]
    public async Task<IActionResult> Remind(
        Guid id,
        [FromBody] GenerateAccessLinkDto? dto,
        [FromServices] RegistrationManagementSendReminderService service,
        CancellationToken cancellationToken)
    {
        // Simply call the existing SendReminder method logic
        return await SendReminder(id, dto, service, cancellationToken);
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
}

public class ValidateRejectDto
{
    public string? Comments { get; set; }
    public string? Reason { get; set; }
}