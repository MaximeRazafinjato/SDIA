using Microsoft.AspNetCore.Mvc;
using SDIA.API.Services;
using SDIA.Application.PublicAccess.RequestCode;
using SDIA.Application.PublicAccess.VerifyCode;
using SDIA.Application.PublicAccess.GetDetails;
using SDIA.Application.PublicAccess.UpdateRegistration;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicAccessController : ControllerBase
{
    private readonly ILogger<PublicAccessController> _logger;
    private readonly INotificationLoggerService _notificationLogger;
    private readonly IConfiguration _configuration;

    public PublicAccessController(
        ILogger<PublicAccessController> logger,
        INotificationLoggerService notificationLogger,
        IConfiguration configuration)
    {
        _logger = logger;
        _notificationLogger = notificationLogger;
        _configuration = configuration;
    }

    /// <summary>
    /// Step 2: When user clicks the email link, verify token and send SMS code
    /// </summary>
    [HttpPost("registration-access/{token}/request-code")]
    public async Task<IActionResult> RequestVerificationCode(
        string token,
        [FromServices] PublicAccessRequestCodeService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ExecuteAsync(token, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new {
                        error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage,
                        requiresPhoneUpdate = true
                    });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            // Send SMS with verification code
            var smsMessage = $@"Bonjour {result.Value.FullName},

Votre code de vérification SDIA est : {result.Value.VerificationCode}

Ce code expire dans {result.Value.ExpiresInMinutes} minutes.

Si vous n'avez pas demandé ce code, veuillez ignorer ce message.";

            await _notificationLogger.LogSmsNotification(
                result.Value.PhoneNumber,
                result.Value.FullName,
                result.Value.VerificationCode,
                null,
                smsMessage
            );

            return Ok(new
            {
                success = true,
                message = $"Un code de vérification a été envoyé au {result.Value.MaskedPhone}",
                phoneNumber = result.Value.MaskedPhone,
                expiresInMinutes = result.Value.ExpiresInMinutes
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
    public async Task<IActionResult> VerifyCode(
        string token,
        [FromBody] VerifyCodeDto dto,
        [FromServices] PublicAccessVerifyCodeService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new PublicAccessVerifyCodeModel
            {
                Token = token,
                Code = dto.Code
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                {
                    var error = result.ValidationErrors.FirstOrDefault();
                    return BadRequest(new {
                        error = error?.ErrorMessage,
                        attemptsRemaining = error?.ErrorMessage?.Contains("tentatives") == true ?
                            int.TryParse(error.ErrorMessage.Split(' ')[error.ErrorMessage.Split(' ').Length - 3], out var attempts) ? attempts : 0 : 0
                    });
                }
                if (result.Errors.Any(e => e.Contains("Trop de tentatives")))
                    return BadRequest(new {
                        error = result.Errors.FirstOrDefault(),
                        maxAttemptsReached = true
                    });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            return Ok(new
            {
                success = result.Value.Success,
                message = result.Value.Message,
                registrationId = result.Value.RegistrationId,
                sessionToken = result.Value.SessionToken,
                redirectUrl = $"/registration-edit/{result.Value.RegistrationId}?session={result.Value.SessionToken}"
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
    public async Task<IActionResult> GetRegistrationDetails(
        Guid id,
        [FromQuery] string sessionToken,
        [FromServices] PublicAccessGetDetailsService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new PublicAccessGetDetailsModel
            {
                Id = id,
                SessionToken = sessionToken
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            return Ok(new
            {
                id = result.Value.Id,
                firstName = result.Value.FirstName,
                lastName = result.Value.LastName,
                email = result.Value.Email,
                phone = result.Value.Phone,
                dateOfBirth = result.Value.DateOfBirth,
                status = result.Value.Status,
                formTemplateId = result.Value.FormTemplateId,
                formTemplateName = result.Value.FormTemplateName,
                formData = result.Value.FormData,
                documents = result.Value.Documents,
                canEdit = result.Value.CanEdit
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
    public async Task<IActionResult> UpdateRegistration(
        Guid id,
        [FromBody] PublicUpdateRegistrationDto dto,
        [FromQuery] string sessionToken,
        [FromServices] PublicAccessUpdateRegistrationService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new PublicAccessUpdateRegistrationModel
            {
                Id = id,
                SessionToken = sessionToken,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                City = dto.City,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                FormData = dto.FormData
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            return Ok(new
            {
                success = result.Value.Success,
                message = result.Value.Message,
                registrationId = result.Value.RegistrationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour" });
        }
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