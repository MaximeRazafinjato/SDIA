using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Public.UpdatePublic;

public class RegistrationPublicUpdateService
{
    private readonly RegistrationPublicUpdateValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicUpdateService(
        RegistrationPublicUpdateValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicUpdateResult>> ExecuteAsync(
        RegistrationPublicUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationPublicUpdateResult>.Invalid(validationResult.ValidationErrors);
        }

        // Get registration directly by access token
        var registration = await _registrationRepository.GetByAccessTokenAsync(model.Token, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationPublicUpdateResult>.NotFound("Registration not found");
        }

        // Update fields
        registration.FirstName = model.FirstName;
        registration.LastName = model.LastName;
        registration.Email = model.Email;
        registration.Phone = model.Phone ?? registration.Phone;
        registration.BirthDate = model.BirthDate;
        registration.FormData = model.FormData ?? registration.FormData;
        registration.UpdatedAt = DateTime.UtcNow;

        // If submit is requested and status is Draft, change to Pending
        if (model.Submit && registration.Status == RegistrationStatus.Draft)
        {
            registration.Status = RegistrationStatus.Pending;
            registration.SubmittedAt = DateTime.UtcNow;
        }

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        var result = new RegistrationPublicUpdateResult
        {
            Success = true,
            Message = model.Submit ? "File submitted successfully" : "Changes saved",
            Status = registration.Status.ToString()
        };

        return Result<RegistrationPublicUpdateResult>.Success(result);
    }
}
