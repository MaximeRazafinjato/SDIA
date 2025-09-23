using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.PublicAccess.CheckStatus;

public class PublicAccessCheckStatusService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessCheckStatusService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessCheckStatusResult>> ExecuteAsync(
        PublicAccessCheckStatusModel model,
        CancellationToken cancellationToken = default)
    {
        // Get registration with form template information
        var registration = await _registrationRepository.GetByIdWithIncludeAsync(
            model.RegistrationId,
            cancellationToken,
            r => r.FormTemplate);

        if (registration == null)
        {
            return Result<PublicAccessCheckStatusResult>.NotFound("Inscription non trouvée");
        }

        // Check if phone is verified (security check)
        if (!registration.PhoneVerified)
        {
            return Result<PublicAccessCheckStatusResult>.Unauthorized("Accès non autorisé. Veuillez vérifier votre téléphone d'abord.");
        }

        var result = MapToResult(registration);
        return Result<PublicAccessCheckStatusResult>.Success(result);
    }

    private static PublicAccessCheckStatusResult MapToResult(Registration registration)
    {
        return new PublicAccessCheckStatusResult
        {
            Id = registration.Id,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            Status = registration.Status.ToString(),
            FormTemplateId = registration.FormTemplateId,
            FormTemplateName = registration.FormTemplate?.Name,
            FormData = registration.FormData,
            CanEdit = registration.Status == RegistrationStatus.Draft ||
                     registration.Status == RegistrationStatus.Pending,
            PhoneVerified = registration.PhoneVerified,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason
        };
    }
}