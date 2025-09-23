using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Validate;

public class RegistrationManagementValidateService
{
    private readonly RegistrationManagementValidateValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementValidateService(
        RegistrationManagementValidateValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementValidateResult>> ExecuteAsync(
        RegistrationManagementValidateModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationManagementValidateResult>.Invalid(validationResult.ValidationErrors);
        }

        // Get the registration
        var registration = await _registrationRepository.GetByIdAsync(model.RegistrationId, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationManagementValidateResult>.NotFound("Registration not found");
        }

        var processedAt = DateTime.UtcNow;

        if (model.IsApproved)
        {
            // Approve the registration
            registration.Status = RegistrationStatus.Validated;
            registration.ValidatedAt = processedAt;
            registration.RejectionReason = string.Empty; // Clear any previous rejection reason
        }
        else
        {
            // Reject the registration
            registration.Status = RegistrationStatus.Rejected;
            registration.RejectedAt = processedAt;
            registration.RejectionReason = model.RejectionReason ?? string.Empty;
        }

        registration.UpdatedAt = processedAt;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // TODO: Add to registration history
        // TODO: Send notification to applicant

        var result = new RegistrationManagementValidateResult
        {
            RegistrationId = registration.Id,
            Status = registration.Status.ToString(),
            ProcessedAt = processedAt,
            IsApproved = model.IsApproved,
            RejectionReason = registration.RejectionReason,
            Success = true,
            Message = model.IsApproved
                ? "Registration approved successfully"
                : "Registration rejected"
        };

        return Result<RegistrationManagementValidateResult>.Success(result);
    }
}