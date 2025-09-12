using Ardalis.Result;
using MediatR;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.GetById;

public class GetRegistrationByIdQueryHandler : IRequestHandler<GetRegistrationByIdQuery, Result<RegistrationDto>>
{
    private readonly IRegistrationRepository _registrationRepository;

    public GetRegistrationByIdQueryHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationDto>> Handle(GetRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var registration = await _registrationRepository.GetByIdAsync(request.RegistrationId, cancellationToken);
        
        if (registration == null)
        {
            return Result<RegistrationDto>.NotFound("Registration not found");
        }

        var registrationDto = MapToRegistrationDto(registration);
        return Result<RegistrationDto>.Success(registrationDto);
    }

    private static RegistrationDto MapToRegistrationDto(Registration registration)
    {
        return new RegistrationDto
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