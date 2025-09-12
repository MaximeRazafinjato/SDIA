using Ardalis.Result;
using MediatR;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Upsert;

public class UpsertRegistrationCommandHandler : IRequestHandler<UpsertRegistrationCommand, Result<RegistrationUpsertResult>>
{
    private readonly IRegistrationRepository _registrationRepository;

    public UpsertRegistrationCommandHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationUpsertResult>> Handle(UpsertRegistrationCommand request, CancellationToken cancellationToken)
    {
        Registration registration;
        bool isCreated;
        
        if (request.IsUpdate)
        {
            registration = await _registrationRepository.GetByIdAsync(request.Id!.Value, cancellationToken);
            if (registration == null)
            {
                return Result<RegistrationUpsertResult>.NotFound("Registration not found");
            }
            isCreated = false;
        }
        else
        {
            registration = new Registration();
            isCreated = true;
            
            // Generate registration number if not provided
            if (string.IsNullOrEmpty(request.RegistrationNumber))
            {
                request.RegistrationNumber = await GenerateRegistrationNumberAsync();
            }
        }

        // Update registration properties
        registration.RegistrationNumber = request.RegistrationNumber;
        registration.Status = request.Status;
        registration.FirstName = request.FirstName;
        registration.LastName = request.LastName;
        registration.Email = request.Email;
        registration.Phone = request.Phone;
        registration.BirthDate = request.BirthDate;
        registration.FormData = request.FormData;
        registration.RejectionReason = request.RejectionReason;
        registration.OrganizationId = request.OrganizationId;
        registration.FormTemplateId = request.FormTemplateId;
        registration.AssignedToUserId = request.AssignedToUserId;

        // Set timestamps based on status
        if (request.Status == RegistrationStatus.Pending && registration.SubmittedAt == null)
        {
            registration.SubmittedAt = DateTime.UtcNow;
        }
        else if (request.Status == RegistrationStatus.Validated && registration.ValidatedAt == null)
        {
            registration.ValidatedAt = DateTime.UtcNow;
        }
        else if (request.Status == RegistrationStatus.Rejected && registration.RejectedAt == null)
        {
            registration.RejectedAt = DateTime.UtcNow;
        }

        if (isCreated)
        {
            await _registrationRepository.AddAsync(registration, cancellationToken);
        }
        else
        {
            await _registrationRepository.UpdateAsync(registration, cancellationToken);
        }

        var result = new RegistrationUpsertResult
        {
            RegistrationId = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            IsCreated = isCreated,
            Message = isCreated ? "Registration created successfully" : "Registration updated successfully"
        };

        return Result<RegistrationUpsertResult>.Success(result);
    }
    
    private async Task<string> GenerateRegistrationNumberAsync()
    {
        // Simple implementation - could be more sophisticated
        var year = DateTime.UtcNow.Year;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"REG-{year}-{timestamp}-{random}";
    }
}