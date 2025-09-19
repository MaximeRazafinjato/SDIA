using Ardalis.Result;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.GetById;

public class RegistrationManagementGetByIdService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementGetByIdService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementGetByIdModel>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByIdAsync(id, cancellationToken);

        if (registration == null)
        {
            return Result<RegistrationManagementGetByIdModel>.NotFound("Registration not found");
        }

        var model = MapToModel(registration);
        return Result<RegistrationManagementGetByIdModel>.Success(model);
    }

    private static RegistrationManagementGetByIdModel MapToModel(Registration registration)
    {
        return new RegistrationManagementGetByIdModel
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            IsMinor = registration.IsMinor,
            FormData = registration.FormData,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason,
            EmailVerified = registration.EmailVerified,
            PhoneVerified = registration.PhoneVerified,
            OrganizationId = registration.OrganizationId,
            OrganizationName = registration.Organization?.Name,
            FormTemplateId = registration.FormTemplateId,
            FormTemplateName = registration.FormTemplate?.Name,
            AssignedToUserId = registration.AssignedToUserId,
            AssignedToUserName = registration.AssignedToUser != null ? $"{registration.AssignedToUser.FirstName} {registration.AssignedToUser.LastName}" : null,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        };
    }
}