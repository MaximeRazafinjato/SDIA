using Ardalis.Result;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.Delete;

public class RegistrationManagementDeleteService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementDeleteService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdAsync(id, cancellationToken);

        if (registration == null)
        {
            return Result.NotFound("Registration not found");
        }

        // Soft delete
        registration.IsDeleted = true;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result.Success();
    }
}