using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SDIA.API.Data;
using SDIA.API.Models.FormTemplates;
using SDIA.Core.FormTemplates;
using System.Security.Claims;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/form-templates")]
public class FormTemplatesController : ControllerBase
{
    private readonly ILogger<FormTemplatesController> _logger;
    private readonly SDIADbContext _context;

    public FormTemplatesController(ILogger<FormTemplatesController> logger, SDIADbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get list of form templates
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFormTemplates(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.FormTemplates
                .Include(ft => ft.Organization)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(ft => ft.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(ft => ft.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ft => new
                {
                    ft.Id,
                    ft.Name,
                    ft.Description,
                    ft.IsActive,
                    ft.CreatedAt,
                    ft.UpdatedAt,
                    JsonConfiguration = ft.FormSchema,
                    OrganizationName = ft.Organization != null ? ft.Organization.Name : null
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
            _logger.LogError(ex, "Error getting form templates");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération des modèles de formulaire" });
        }
    }

    /// <summary>
    /// Get form template by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetFormTemplate(Guid id)
    {
        try
        {
            var template = await _context.FormTemplates
                .Include(ft => ft.Organization)
                .FirstOrDefaultAsync(ft => ft.Id == id);

            if (template == null)
            {
                return NotFound(new { error = "Modèle de formulaire non trouvé" });
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form template {TemplateId}", id);
            return StatusCode(500, new { error = "Une erreur est survenue lors de la récupération du modèle de formulaire" });
        }
    }

    /// <summary>
    /// Create a new form template
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFormTemplate([FromBody] CreateFormTemplateDto dto)
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

            var template = new FormTemplate
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                FormSchema = dto.JsonConfiguration,
                IsActive = dto.IsActive,
                OrganizationId = user.OrganizationId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.FormTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormTemplate), new { id = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form template");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la création du modèle de formulaire" });
        }
    }

    /// <summary>
    /// Update a form template
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFormTemplate(Guid id, [FromBody] UpdateFormTemplateDto dto)
    {
        try
        {
            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound(new { error = "Modèle de formulaire non trouvé" });
            }

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.FormSchema = dto.JsonConfiguration;
            template.IsActive = dto.IsActive;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Modèle de formulaire mis à jour avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form template");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la mise à jour du modèle de formulaire" });
        }
    }

    /// <summary>
    /// Delete a form template
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFormTemplate(Guid id)
    {
        try
        {
            var template = await _context.FormTemplates
                .Include(ft => ft.Registrations)
                .FirstOrDefaultAsync(ft => ft.Id == id);
                
            if (template == null)
            {
                return NotFound(new { error = "Modèle de formulaire non trouvé" });
            }

            if (template.Registrations.Any())
            {
                return BadRequest(new { error = "Impossible de supprimer un modèle utilisé par des inscriptions" });
            }

            _context.FormTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Modèle de formulaire supprimé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form template");
            return StatusCode(500, new { error = "Une erreur est survenue lors de la suppression du modèle de formulaire" });
        }
    }
}