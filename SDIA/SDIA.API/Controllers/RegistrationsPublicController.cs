using Microsoft.AspNetCore.Mvc;
using SDIA.API.Models.Registrations;
using SDIA.Application.Registrations.Public.VerifyAccess;
using SDIA.Application.Registrations.Public.GetPublic;
using SDIA.Application.Registrations.Public.UpdatePublic;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations-public")]
public class RegistrationsPublicController : ControllerBase
{
    private readonly ILogger<RegistrationsPublicController> _logger;

    public RegistrationsPublicController(ILogger<RegistrationsPublicController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Verify access code and get registration data
    /// </summary>
    [HttpPost("verify-access")]
    public async Task<IActionResult> VerifyAccess(
        [FromBody] VerifyAccessDto dto,
        [FromServices] RegistrationPublicVerifyAccessService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = new RegistrationPublicVerifyAccessModel
            {
                Token = dto.Token,
                Code = dto.Code
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            // Set session after successful verification
            HttpContext.Session.SetString($"registration_access_{result.Value.RegistrationId}", result.Value.SessionToken);
            HttpContext.Session.SetString($"registration_token_{model.Token}", result.Value.RegistrationId.ToString());

            return Ok(new {
                success = result.Value.Success,
                sessionToken = result.Value.SessionToken,
                registrationId = result.Value.RegistrationId,
                message = result.Value.Message
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
    public async Task<IActionResult> GetPublicRegistration(
        string token,
        [FromHeader(Name = "X-Session-Token")] string? sessionToken,
        [FromServices] RegistrationPublicGetService service,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify session in controller
            var registrationIdStr = HttpContext.Session.GetString($"registration_token_{token}");
            if (string.IsNullOrEmpty(registrationIdStr))
            {
                return Unauthorized(new { error = "Session expired. Please verify your code again." });
            }

            var registrationId = Guid.Parse(registrationIdStr);
            var storedSessionToken = HttpContext.Session.GetString($"registration_access_{registrationId}");

            if (string.IsNullOrEmpty(storedSessionToken) || storedSessionToken != sessionToken)
            {
                return Unauthorized(new { error = "Invalid session" });
            }

            var model = new RegistrationPublicGetModel
            {
                Token = token,
                SessionToken = sessionToken
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            // Map to the expected DTO format
            var dto = MapToPublicDto(result.Value);
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
        [FromHeader(Name = "X-Session-Token")] string? sessionToken,
        [FromServices] RegistrationPublicUpdateService service,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify session in controller
            var registrationIdStr = HttpContext.Session.GetString($"registration_token_{token}");
            if (string.IsNullOrEmpty(registrationIdStr))
            {
                return Unauthorized(new { error = "Session expired. Please verify your code again." });
            }

            var registrationId = Guid.Parse(registrationIdStr);
            var storedSessionToken = HttpContext.Session.GetString($"registration_access_{registrationId}");

            if (string.IsNullOrEmpty(storedSessionToken) || storedSessionToken != sessionToken)
            {
                return Unauthorized(new { error = "Invalid session" });
            }

            var model = new RegistrationPublicUpdateModel
            {
                Token = token,
                SessionToken = sessionToken,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                BirthDate = dto.BirthDate,
                FormData = dto.FormData,
                Submit = dto.Submit
            };

            var result = await service.ExecuteAsync(model, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                    return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
                if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                    return NotFound(new { error = result.Errors.FirstOrDefault() });
                return BadRequest(new { error = result.Errors.FirstOrDefault() });
            }

            return Ok(new {
                success = result.Value.Success,
                message = result.Value.Message,
                status = result.Value.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating public registration");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour" });
        }
    }

    private static PublicRegistrationDto MapToPublicDto(RegistrationPublicGetResult result)
    {
        return new PublicRegistrationDto
        {
            Id = result.Id,
            RegistrationNumber = result.RegistrationNumber,
            Status = result.Status,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Email = result.Email,
            Phone = result.Phone,
            BirthDate = result.BirthDate,
            FormData = result.FormData,
            CreatedAt = result.CreatedAt,
            SubmittedAt = result.SubmittedAt,
            ValidatedAt = result.ValidatedAt,
            RejectedAt = result.RejectedAt,
            RejectionReason = result.RejectionReason,
            OrganizationName = result.OrganizationName,
            FormTemplateName = result.FormTemplateName,
            IsMinor = result.IsMinor,
            Comments = result.Comments?.Select(c => new RegistrationCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorName = c.AuthorName,
                IsInternal = c.IsInternal
            }).ToList(),
            Documents = result.Documents?.Select(d => new DocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                DocumentType = d.DocumentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt
            }).ToList()
        };
    }

}