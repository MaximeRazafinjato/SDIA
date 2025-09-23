using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Public.GetPublic;

public class RegistrationPublicGetService
{
    private readonly RegistrationPublicGetValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicGetService(
        RegistrationPublicGetValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicGetResult>> ExecuteAsync(
        RegistrationPublicGetModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationPublicGetResult>.Invalid(validationResult.ValidationErrors);
        }

        // Get registration with all related data using the access token directly
        var registration = await _registrationRepository.GetByAccessTokenAsync(model.Token, cancellationToken);

        if (registration == null)
        {
            return Result<RegistrationPublicGetResult>.NotFound("Registration not found");
        }

        var result = MapToResult(registration);
        return Result<RegistrationPublicGetResult>.Success(result);
    }

    private static RegistrationPublicGetResult MapToResult(Registration registration)
    {
        return new RegistrationPublicGetResult
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            FormData = registration.FormData,
            CreatedAt = registration.CreatedAt,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason,
            OrganizationName = registration.Organization?.Name ?? "",
            FormTemplateName = registration.FormTemplate?.Name,
            IsMinor = registration.IsMinor,
            Comments = registration.Comments?.Where(c => !c.IsInternal).Select(c => new RegistrationPublicCommentResult
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorName = c.Author?.FirstName + " " + c.Author?.LastName ?? "System",
                IsInternal = c.IsInternal
            }).ToList(),
            Documents = registration.Documents?.Select(d => new RegistrationPublicDocumentResult
            {
                Id = d.Id,
                FileName = d.FileName,
                DocumentType = d.DocumentType,
                FileSize = d.FileSize,
                UploadedAt = d.CreatedAt
            }).ToList()
        };
    }
}
