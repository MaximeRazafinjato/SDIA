using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using System.Text.Json;

namespace SDIA.Application.PublicAccess.GetDetails;

public class PublicAccessGetDetailsService
{
    private readonly IRegistrationRepository _registrationRepository;

    public PublicAccessGetDetailsService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<PublicAccessGetDetailsResult>> ExecuteAsync(
        PublicAccessGetDetailsModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdWithIncludeAsync(
            model.Id,
            cancellationToken,
            r => r.FormTemplate);

        if (registration == null || !registration.PhoneVerified)
        {
            return Result<PublicAccessGetDetailsResult>.NotFound("Inscription non trouvée ou accès non autorisé");
        }

        var result = new PublicAccessGetDetailsResult
        {
            Id = registration.Id,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            DateOfBirth = registration.BirthDate,
            Status = registration.Status.ToString(),
            FormTemplateId = registration.FormTemplateId,
            FormTemplateName = registration.FormTemplate?.Name,
            FormData = registration.FormData,
            Documents = registration.Documents?.Any() == true ?
                JsonSerializer.Serialize(registration.Documents.Select(d => new {
                    d.Id,
                    d.FileName,
                    FileType = d.ContentType,
                    d.FileSize,
                    UploadedAt = d.CreatedAt
                })) : null,
            CanEdit = registration.Status == RegistrationStatus.Draft ||
                     registration.Status == RegistrationStatus.Pending
        };

        return Result<PublicAccessGetDetailsResult>.Success(result);
    }
}