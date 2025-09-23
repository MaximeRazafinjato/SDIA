using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDIA.API.Models.FormTemplates;
using SDIA.Application.FormTemplates.Management.Grid;
using SDIA.Application.FormTemplates.Management.GetAll;
using SDIA.Application.FormTemplates.Management.GetById;
using SDIA.Application.FormTemplates.Management.Upsert;
using SDIA.Application.FormTemplates.Management.Delete;
using System.Security.Claims;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/form-templates")]
public class FormTemplatesController : ControllerBase
{
    private readonly ILogger<FormTemplatesController> _logger;

    public FormTemplatesController(ILogger<FormTemplatesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get form templates grid (POST)
    /// </summary>
    [HttpPost("grid")]
    [Authorize]
    public async Task<IActionResult> GetGrid(
        [FromBody] FormTemplateManagementGridQuery query,
        [FromServices] FormTemplateManagementGridService service,
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
    /// Get list of form templates
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFormTemplates(
        [FromQuery] bool? isActive,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] FormTemplateManagementGetAllService service,
        CancellationToken cancellationToken)
    {
        var query = new FormTemplateManagementGetAllQuery
        {
            IsActive = isActive,
            Page = page == 0 ? 1 : page,
            PageSize = pageSize == 0 ? 20 : pageSize
        };

        var result = await service.ExecuteAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get form template by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetFormTemplate(
        Guid id,
        [FromServices] FormTemplateManagementGetByIdService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                return NotFound(new { error = result.Errors.FirstOrDefault() });
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new form template
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFormTemplate(
        [FromBody] CreateFormTemplateDto dto,
        [FromServices] FormTemplateManagementUpsertService service,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var model = new FormTemplateManagementUpsertModel
        {
            Name = dto.Name,
            Description = dto.Description,
            FormSchema = dto.JsonConfiguration,
            IsActive = dto.IsActive,
            OrganizationId = Guid.Empty // Will be set from user context in service
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                return BadRequest(result.ValidationErrors);
            return BadRequest(result.Errors);
        }

        return CreatedAtAction(nameof(GetFormTemplate), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Update a form template
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFormTemplate(
        Guid id,
        [FromBody] UpdateFormTemplateDto dto,
        [FromServices] FormTemplateManagementUpsertService service,
        CancellationToken cancellationToken)
    {
        var model = new FormTemplateManagementUpsertModel
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            FormSchema = dto.JsonConfiguration,
            IsActive = dto.IsActive
        };

        var result = await service.ExecuteAsync(model, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                return NotFound(new { error = result.Errors.FirstOrDefault() });
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                return BadRequest(result.ValidationErrors);
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Modèle de formulaire mis à jour avec succès", data = result.Value });
    }

    /// <summary>
    /// Delete a form template
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFormTemplate(
        Guid id,
        [FromServices] FormTemplateManagementDeleteService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                return NotFound(new { error = result.Errors.FirstOrDefault() });
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                return BadRequest(new { error = result.ValidationErrors.FirstOrDefault()?.ErrorMessage });
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Modèle de formulaire supprimé avec succès" });
    }
}