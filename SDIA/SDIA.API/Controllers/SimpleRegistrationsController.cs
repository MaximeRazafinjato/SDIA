using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDIA.API.Models.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.API.Controllers;

[ApiController]
[Route("api/registrations")]
public class SimpleRegistrationsController : ControllerBase
{
    private readonly ILogger<SimpleRegistrationsController> _logger;
    private static readonly List<object> _mockRegistrations = new();

    public SimpleRegistrationsController(ILogger<SimpleRegistrationsController> logger)
    {
        _logger = logger;
        
        // Initialize with mock data if empty
        if (!_mockRegistrations.Any())
        {
            _mockRegistrations.Add(new
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = "REG-20250911-ABC123",
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@example.com",
                Phone = "+33123456789",
                Status = RegistrationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                OrganizationName = "SDIA Demo Organization",
                IsMinor = false,
                AssignedToUserName = "Admin User"
            });
            
            _mockRegistrations.Add(new
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = "REG-20250910-DEF456",
                FirstName = "Marie",
                LastName = "Martin",
                Email = "marie.martin@example.com",
                Phone = "+33987654321",
                Status = RegistrationStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                SubmittedAt = (DateTime?)null,
                OrganizationName = "SDIA Demo Organization",
                IsMinor = true,
                AssignedToUserName = (string)null
            });
            
            _mockRegistrations.Add(new
            {
                Id = Guid.NewGuid(),
                RegistrationNumber = "REG-20250909-GHI789",
                FirstName = "Pierre",
                LastName = "Bernard",
                Email = "pierre.bernard@example.com",
                Phone = "+33555666777",
                Status = RegistrationStatus.Validated,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                SubmittedAt = DateTime.UtcNow.AddDays(-8),
                OrganizationName = "SDIA Demo Organization",
                IsMinor = false,
                AssignedToUserName = "Manager User"
            });
        }
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
            await Task.Delay(50); // Simulate async operation
            
            // Calculate real statistics from mock data
            var pendingCount = _mockRegistrations.Count(r => ((dynamic)r).Status == RegistrationStatus.Pending);
            var validatedCount = _mockRegistrations.Count(r => ((dynamic)r).Status == RegistrationStatus.Validated);
            var rejectedCount = _mockRegistrations.Count(r => ((dynamic)r).Status == RegistrationStatus.Rejected);
            var draftCount = _mockRegistrations.Count(r => ((dynamic)r).Status == RegistrationStatus.Draft);
            
            return Ok(new
            {
                total = _mockRegistrations.Count,
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
            return StatusCode(500, new { message = "An error occurred while fetching statistics" });
        }
    }

    /// <summary>
    /// Get list of registrations
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRegistrations(
        [FromQuery] Guid? organizationId = null,
        [FromQuery] RegistrationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            await Task.Delay(100); // Simulate async operation
            
            var items = _mockRegistrations.ToList();
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
            _logger.LogError(ex, "Error getting registrations");
            return StatusCode(500, new { message = "An error occurred while fetching registrations" });
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
            await Task.Delay(50); // Simulate async operation
            
            var registration = _mockRegistrations.FirstOrDefault();
            if (registration == null)
            {
                return NotFound();
            }

            return Ok(registration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Create a new registration
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Task.Delay(100); // Simulate async operation

            var registrationId = Guid.NewGuid();
            var registrationNumber = $"REG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var newRegistration = new
            {
                Id = registrationId,
                RegistrationNumber = registrationNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Status = RegistrationStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                SubmittedAt = (DateTime?)null,
                OrganizationName = "SDIA Demo Organization",
                IsMinor = false,
                AssignedToUserName = (string)null
            };

            _mockRegistrations.Add(newRegistration);

            return Ok(new
            {
                id = registrationId,
                registrationNumber,
                message = "Registration created successfully. Please verify your email and phone."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating registration");
            return StatusCode(500, new { message = "An error occurred while creating the registration" });
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Updating registration {Id} status to {Status}", id, dto.Status);

            return Ok(new { message = "Status updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registration status for {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Verify email address
    /// </summary>
    [HttpPost("{id}/verify-email")]
    public async Task<IActionResult> VerifyEmail(Guid id, [FromBody] VerifyEmailDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Email verification for registration {Id} with token {Token}", id, dto.Token);

            return Ok(new { message = "Email verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Verify phone number
    /// </summary>
    [HttpPost("{id}/verify-phone")]
    public async Task<IActionResult> VerifyPhone(Guid id, [FromBody] VerifyPhoneDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Task.Delay(100); // Simulate async operation

            _logger.LogInformation("Phone verification for registration {Id} with code {Code}", id, dto.Code);

            return Ok(new { message = "Phone verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying phone for {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}