using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Public.GetByIdentifier;

public class RegistrationPublicGetByIdentifierService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicGetByIdentifierService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicGetByIdentifierResult>> ExecuteAsync(
        RegistrationPublicGetByIdentifierModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Identifier))
        {
            return Result<RegistrationPublicGetByIdentifierResult>.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Identifier", ErrorMessage = "Identifier is required" } });
        }

        Registration? registration = null;

        // Try to find by access token first (for public access)
        registration = await _registrationRepository.GetByTokenAsync(model.Identifier);

        // If not found, try by registration number
        if (registration == null)
        {
            var allRegistrations = _registrationRepository.GetAll(cancellationToken);
            registration = allRegistrations.FirstOrDefault(r => r.RegistrationNumber == model.Identifier);
        }

        if (registration == null)
        {
            return Result<RegistrationPublicGetByIdentifierResult>.NotFound("Registration not found");
        }

        // Load related data
        registration = await _registrationRepository.GetByIdWithIncludeAsync(
            registration.Id,
            cancellationToken,
            r => r.FormTemplate);

        if (registration == null)
        {
            return Result<RegistrationPublicGetByIdentifierResult>.NotFound("Registration not found");
        }

        var result = MapToResult(registration);
        return Result<RegistrationPublicGetByIdentifierResult>.Success(result);
    }

    private static RegistrationPublicGetByIdentifierResult MapToResult(Registration registration)
    {
        return new RegistrationPublicGetByIdentifierResult
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            Status = registration.Status.ToString(),
            FormTemplateName = registration.FormTemplate?.Name,
            FormData = registration.FormData,
            CanEdit = registration.Status == RegistrationStatus.Draft ||
                     registration.Status == RegistrationStatus.Pending,
            EmailVerified = registration.EmailVerified,
            PhoneVerified = registration.PhoneVerified,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason,
            CreatedAt = registration.CreatedAt
        };
    }
}