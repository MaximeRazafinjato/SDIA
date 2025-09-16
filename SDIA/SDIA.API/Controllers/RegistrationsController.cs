using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SDIA.API.Data;
using SDIA.API.Models.Registrations;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using System.Security.Claims;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly ILogger<RegistrationsController> _logger;
    private readonly SDIADbContext _context;

    public RegistrationsController(ILogger<RegistrationsController> logger, SDIADbContext context)
    {
        _logger = logger;
        _context = context;
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
            var stats = await _context.Registrations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

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
}