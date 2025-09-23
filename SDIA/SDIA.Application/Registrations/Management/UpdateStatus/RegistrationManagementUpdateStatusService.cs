using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.UpdateStatus;

public class RegistrationManagementUpdateStatusService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementUpdateStatusService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ExecuteAsync(
        RegistrationManagementUpdateStatusModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r => r.Id == model.Id, cancellationToken);

        if (registration == null)
        {
            return Result.NotFound("Inscription non trouvée");
        }

        registration.Status = model.Status;
        registration.UpdatedAt = DateTime.UtcNow;

        if (model.Status == RegistrationStatus.Pending && !registration.SubmittedAt.HasValue)
        {
            registration.SubmittedAt = DateTime.UtcNow;
        }

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result.Success();
    }
}

public class RegistrationManagementUpdateStatusModel
{
    public Guid Id { get; set; }
    public RegistrationStatus Status { get; set; }
}