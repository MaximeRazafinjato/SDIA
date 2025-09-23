using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Update;

public class RegistrationManagementUpdateService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementUpdateService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementUpdateResult>> ExecuteAsync(
        RegistrationManagementUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r => r.Id == model.Id, cancellationToken);

        if (registration == null)
        {
            return Result<RegistrationManagementUpdateResult>.NotFound("Inscription non trouvée");
        }

        // Update fields
        registration.FirstName = model.FirstName ?? registration.FirstName;
        registration.LastName = model.LastName ?? registration.LastName;
        registration.Email = model.Email ?? registration.Email;
        registration.Phone = model.Phone ?? registration.Phone;
        registration.BirthDate = model.BirthDate ?? registration.BirthDate;
        registration.FormData = model.FormData ?? registration.FormData;
        registration.UpdatedAt = DateTime.UtcNow;

        // Update status if provided
        if (model.Status.HasValue)
        {
            registration.Status = model.Status.Value;

            if (model.Status == RegistrationStatus.Validated && !registration.ValidatedAt.HasValue)
            {
                registration.ValidatedAt = DateTime.UtcNow;
            }
            else if (model.Status == RegistrationStatus.Rejected && !registration.RejectedAt.HasValue)
            {
                registration.RejectedAt = DateTime.UtcNow;
                registration.RejectionReason = model.RejectionReason ?? "Dossier non conforme";
            }
            else if (model.Status == RegistrationStatus.Pending && !registration.SubmittedAt.HasValue)
            {
                registration.SubmittedAt = DateTime.UtcNow;
            }
        }

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new RegistrationManagementUpdateResult
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status.ToString(),
            UpdatedAt = registration.UpdatedAt.Value,
            Message = "Inscription mise à jour avec succès"
        };

        return Result<RegistrationManagementUpdateResult>.Success(result);
    }
}

public class RegistrationManagementUpdateModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? FormData { get; set; }
    public RegistrationStatus? Status { get; set; }
    public string? RejectionReason { get; set; }
}

public class RegistrationManagementUpdateResult
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}