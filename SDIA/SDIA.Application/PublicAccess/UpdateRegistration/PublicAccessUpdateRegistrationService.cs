using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.PublicAccess.UpdateRegistration;

public class PublicAccessUpdateRegistrationService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessUpdateRegistrationService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessUpdateRegistrationResult>> ExecuteAsync(
        PublicAccessUpdateRegistrationModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdAsync(model.Id, cancellationToken);

        if (registration == null || !registration.PhoneVerified)
        {
            return Result<PublicAccessUpdateRegistrationResult>.NotFound("Inscription non trouvée ou accès non autorisé");
        }

        // Only allow editing if status is Draft or Pending
        if (registration.Status != RegistrationStatus.Draft &&
            registration.Status != RegistrationStatus.Pending)
        {
            return Result<PublicAccessUpdateRegistrationResult>.Error("Cette inscription ne peut plus être modifiée");
        }

        // Update fields
        if (!string.IsNullOrEmpty(model.FirstName)) registration.FirstName = model.FirstName;
        if (!string.IsNullOrEmpty(model.LastName)) registration.LastName = model.LastName;
        if (!string.IsNullOrEmpty(model.Email)) registration.Email = model.Email;
        if (!string.IsNullOrEmpty(model.Phone)) registration.Phone = model.Phone;
        if (model.DateOfBirth.HasValue) registration.BirthDate = model.DateOfBirth.Value;
        if (!string.IsNullOrEmpty(model.FormData)) registration.FormData = model.FormData;

        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new PublicAccessUpdateRegistrationResult
        {
            Success = true,
            Message = "Inscription mise à jour avec succès",
            RegistrationId = registration.Id
        };

        return Result<PublicAccessUpdateRegistrationResult>.Success(result);
    }
}