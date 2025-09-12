using Ardalis.Result;
using MediatR;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.Grid;

public class GetRegistrationsGridQueryHandler : IRequestHandler<GetRegistrationsGridQuery, Result<GridResult<RegistrationGridDto>>>
{
    private readonly IRegistrationRepository _registrationRepository;

    public GetRegistrationsGridQueryHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<GridResult<RegistrationGridDto>>> Handle(GetRegistrationsGridQuery request, CancellationToken cancellationToken)
    {
        var query = await _registrationRepository.GetQueryableAsync();
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                x.Phone.Contains(searchTerm) ||
                x.RegistrationNumber.ToLower().Contains(searchTerm));
        }
        
        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }
        
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == request.OrganizationId.Value);
        }
        
        if (request.FormTemplateId.HasValue)
        {
            query = query.Where(x => x.FormTemplateId == request.FormTemplateId.Value);
        }
        
        if (request.AssignedToUserId.HasValue)
        {
            query = query.Where(x => x.AssignedToUserId == request.AssignedToUserId.Value);
        }
        
        if (request.SubmittedFrom.HasValue)
        {
            query = query.Where(x => x.SubmittedAt >= request.SubmittedFrom.Value);
        }
        
        if (request.SubmittedTo.HasValue)
        {
            query = query.Where(x => x.SubmittedAt <= request.SubmittedTo.Value);
        }
        
        if (request.EmailVerified.HasValue)
        {
            query = query.Where(x => x.EmailVerified == request.EmailVerified.Value);
        }
        
        if (request.PhoneVerified.HasValue)
        {
            query = query.Where(x => x.PhoneVerified == request.PhoneVerified.Value);
        }

        // Map to DTOs
        var mappedQuery = query.Select(x => MapToRegistrationGridDto(x));
        
        var result = await mappedQuery.ToGridResultAsync(request, cancellationToken);
        
        return Result<GridResult<RegistrationGridDto>>.Success(result);
    }

    private static RegistrationGridDto MapToRegistrationGridDto(Registration registration)
    {
        return new RegistrationGridDto
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
            SubmittedAt = registration.SubmittedAt,
            EmailVerified = registration.EmailVerified,
            PhoneVerified = registration.PhoneVerified,
            OrganizationName = registration.Organization?.Name,
            FormTemplateName = registration.FormTemplate?.Name,
            AssignedToUserName = registration.AssignedToUser != null ? $"{registration.AssignedToUser.FirstName} {registration.AssignedToUser.LastName}" : null,
            CreatedAt = registration.CreatedAt
        };
    }
}