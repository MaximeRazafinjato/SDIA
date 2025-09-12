using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDIA.API.Models.FormTemplates;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/form-templates")]
public class SimpleFormTemplatesController : ControllerBase
{
    private readonly ILogger<SimpleFormTemplatesController> _logger;
    private static readonly List<object> _mockTemplates = new();

    public SimpleFormTemplatesController(ILogger<SimpleFormTemplatesController> logger)
    {
        _logger = logger;
        
        // Initialize with mock data if empty
        if (!_mockTemplates.Any())
        {
            _mockTemplates.Add(new
            {
                Id = Guid.NewGuid(),
                Name = "Formulaire d'inscription standard",
                Description = "Formulaire standard pour les nouvelles inscriptions",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                JsonConfiguration = """
                    {
                        "fields": [
                            {
                                "id": "firstName",
                                "type": "text",
                                "label": "Prénom",
                                "required": true,
                                "placeholder": "Votre prénom"
                            },
                            {
                                "id": "lastName",
                                "type": "text",
                                "label": "Nom",
                                "required": true,
                                "placeholder": "Votre nom"
                            },
                            {
                                "id": "email",
                                "type": "email",
                                "label": "Email",
                                "required": true,
                                "placeholder": "votre.email@example.com"
                            },
                            {
                                "id": "phone",
                                "type": "tel",
                                "label": "Téléphone",
                                "placeholder": "+33123456789"
                            },
                            {
                                "id": "birthDate",
                                "type": "date",
                                "label": "Date de naissance",
                                "required": true
                            }
                        ]
                    }
                    """
            });
        }
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
            await Task.Delay(100); // Simulate async operation
            
            var items = _mockTemplates.ToList();
            var totalCount = items.Count;
            
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
            return StatusCode(500, new { message = "An error occurred while fetching form templates" });
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
            await Task.Delay(50); // Simulate async operation
            
            var template = _mockTemplates.FirstOrDefault();
            if (template == null)
            {
                return NotFound();
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form template {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create a new form template
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateFormTemplate([FromBody] CreateFormTemplateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate JSON configuration
            try
            {
                System.Text.Json.JsonDocument.Parse(dto.JsonConfiguration);
            }
            catch (System.Text.Json.JsonException)
            {
                return BadRequest(new { message = "Invalid JSON configuration format" });
            }

            await Task.Delay(100); // Simulate async operation

            var templateId = Guid.NewGuid();
            var newTemplate = new
            {
                Id = templateId,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = (DateTime?)null,
                JsonConfiguration = dto.JsonConfiguration
            };

            _mockTemplates.Add(newTemplate);

            return CreatedAtAction(
                nameof(GetFormTemplate),
                new { id = templateId },
                new { id = templateId, message = "Form template created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form template");
            return StatusCode(500, new { message = "An error occurred while creating the form template" });
        }
    }

    /// <summary>
    /// Update form template
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateFormTemplate(Guid id, [FromBody] UpdateFormTemplateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate JSON configuration
            try
            {
                System.Text.Json.JsonDocument.Parse(dto.JsonConfiguration);
            }
            catch (System.Text.Json.JsonException)
            {
                return BadRequest(new { message = "Invalid JSON configuration format" });
            }

            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Updating form template {Id}", id);

            return Ok(new { message = "Form template updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form template {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the form template" });
        }
    }

    /// <summary>
    /// Delete form template
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteFormTemplate(Guid id)
    {
        try
        {
            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Deleting form template {Id}", id);

            return Ok(new { message = "Form template deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form template {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the form template" });
        }
    }

    /// <summary>
    /// Get public form template (no authentication required)
    /// </summary>
    [HttpGet("public/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicFormTemplate(Guid id)
    {
        try
        {
            await Task.Delay(50); // Simulate async operation
            
            var template = _mockTemplates.FirstOrDefault();
            if (template == null)
            {
                return NotFound();
            }

            // Return only public information needed for form rendering
            var publicTemplate = new
            {
                ((dynamic)template).Id,
                ((dynamic)template).Name,
                ((dynamic)template).Description,
                ((dynamic)template).JsonConfiguration
            };

            return Ok(publicTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public form template {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}