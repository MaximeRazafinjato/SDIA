using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Submit;

public class RegistrationManagementSubmitService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementSubmitService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementSubmitResult>> ExecuteAsync(
        RegistrationManagementSubmitModel model,
        CancellationToken cancellationToken = default)
    {
        // Get the registration
        var registration = await _registrationRepository.GetByIdAsync(model.RegistrationId, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationManagementSubmitResult>.NotFound("Registration not found");
        }

        // Check if registration can be submitted
        if (registration.Status != RegistrationStatus.Draft)
        {
            return Result<RegistrationManagementSubmitResult>.Error("Only draft registrations can be submitted");
        }

        // Validate required data
        if (string.IsNullOrEmpty(registration.FirstName) ||
            string.IsNullOrEmpty(registration.LastName) ||
            string.IsNullOrEmpty(registration.Email))
        {
            return Result<RegistrationManagementSubmitResult>.Error("Missing required registration data");
        }

        // Update status and submission date
        registration.Status = RegistrationStatus.Pending;
        registration.SubmittedAt = DateTime.UtcNow;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // TODO: Add to registration history
        // TODO: Send notification emails

        var result = new RegistrationManagementSubmitResult
        {
            RegistrationId = registration.Id,
            Status = registration.Status.ToString(),
            SubmittedAt = registration.SubmittedAt.Value,
            Success = true,
            Message = "Registration submitted successfully"
        };

        return Result<RegistrationManagementSubmitResult>.Success(result);
    }
}