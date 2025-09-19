using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using System.Text.RegularExpressions;

namespace SDIA.Application.Registrations.Management.Upsert;

public class RegistrationManagementUpsertService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementUpsertService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementUpsertResult>> ExecuteAsync(RegistrationManagementUpsertModel model, CancellationToken cancellationToken = default)
    {
        // Manual validation
        var validationErrors = new List<ValidationError>();

        // FirstName validation
        if (string.IsNullOrWhiteSpace(model.FirstName))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.FirstName), ErrorMessage = "First name is required" });
        else if (model.FirstName.Length > 100)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.FirstName), ErrorMessage = "First name must not exceed 100 characters" });

        // LastName validation
        if (string.IsNullOrWhiteSpace(model.LastName))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.LastName), ErrorMessage = "Last name is required" });
        else if (model.LastName.Length > 100)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.LastName), ErrorMessage = "Last name must not exceed 100 characters" });

        // Email validation
        if (string.IsNullOrWhiteSpace(model.Email))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Email), ErrorMessage = "Email is required" });
        else
        {
            if (model.Email.Length > 255)
                validationErrors.Add(new ValidationError { Identifier = nameof(model.Email), ErrorMessage = "Email must not exceed 255 characters" });

            if (!IsValidEmail(model.Email))
                validationErrors.Add(new ValidationError { Identifier = nameof(model.Email), ErrorMessage = "Invalid email format" });
        }

        // Phone validation
        if (string.IsNullOrWhiteSpace(model.Phone))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Phone), ErrorMessage = "Phone is required" });
        else if (!Regex.IsMatch(model.Phone, @"^\+?[1-9]\d{1,14}$"))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Phone), ErrorMessage = "Invalid phone format" });

        // BirthDate validation
        if (model.BirthDate == default)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.BirthDate), ErrorMessage = "Birth date is required" });
        else
        {
            if (model.BirthDate > DateTime.Today)
                validationErrors.Add(new ValidationError { Identifier = nameof(model.BirthDate), ErrorMessage = "Birth date cannot be in the future" });

            if (model.BirthDate < DateTime.Today.AddYears(-120))
                validationErrors.Add(new ValidationError { Identifier = nameof(model.BirthDate), ErrorMessage = "Birth date is not valid" });
        }

        // Status validation
        if (!Enum.IsDefined(typeof(RegistrationStatus), model.Status))
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Status), ErrorMessage = "Invalid registration status" });

        // OrganizationId validation
        if (model.OrganizationId == Guid.Empty)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.OrganizationId), ErrorMessage = "Organization ID is required" });

        // FormTemplateId validation
        if (model.FormTemplateId == Guid.Empty)
            validationErrors.Add(new ValidationError { Identifier = nameof(model.FormTemplateId), ErrorMessage = "Form template ID is required" });

        // Rejection reason validation for rejected status
        if (model.Status == RegistrationStatus.Rejected)
        {
            if (string.IsNullOrWhiteSpace(model.RejectionReason))
                validationErrors.Add(new ValidationError { Identifier = nameof(model.RejectionReason), ErrorMessage = "Rejection reason is required when status is Rejected" });
            else if (model.RejectionReason.Length > 1000)
                validationErrors.Add(new ValidationError { Identifier = nameof(model.RejectionReason), ErrorMessage = "Rejection reason must not exceed 1000 characters" });
        }

        // Registration number validation for updates
        if (model.IsUpdate)
        {
            if (string.IsNullOrWhiteSpace(model.RegistrationNumber))
                validationErrors.Add(new ValidationError { Identifier = nameof(model.RegistrationNumber), ErrorMessage = "Registration number is required for updates" });
            else if (model.RegistrationNumber.Length > 50)
                validationErrors.Add(new ValidationError { Identifier = nameof(model.RegistrationNumber), ErrorMessage = "Registration number must not exceed 50 characters" });
        }

        if (validationErrors.Any())
            return Result<RegistrationManagementUpsertResult>.Invalid(validationErrors);

        Registration registration;
        bool isCreated;

        if (model.IsUpdate)
        {
            registration = await _registrationRepository.GetByIdAsync(model.Id!.Value, cancellationToken);
            if (registration == null)
            {
                return Result<RegistrationManagementUpsertResult>.NotFound("Registration not found");
            }
            isCreated = false;
        }
        else
        {
            registration = new Registration();
            isCreated = true;

            // Generate registration number if not provided
            if (string.IsNullOrEmpty(model.RegistrationNumber))
            {
                model.RegistrationNumber = await GenerateRegistrationNumberAsync();
            }
        }

        // Update registration properties
        registration.RegistrationNumber = model.RegistrationNumber;
        registration.Status = model.Status;
        registration.FirstName = model.FirstName;
        registration.LastName = model.LastName;
        registration.Email = model.Email;
        registration.Phone = model.Phone;
        registration.BirthDate = model.BirthDate;
        registration.FormData = model.FormData;
        registration.RejectionReason = model.RejectionReason;
        registration.OrganizationId = model.OrganizationId;
        registration.FormTemplateId = model.FormTemplateId;
        registration.AssignedToUserId = model.AssignedToUserId;

        // Set timestamps based on status
        if (model.Status == RegistrationStatus.Pending && registration.SubmittedAt == null)
        {
            registration.SubmittedAt = DateTime.UtcNow;
        }
        else if (model.Status == RegistrationStatus.Validated && registration.ValidatedAt == null)
        {
            registration.ValidatedAt = DateTime.UtcNow;
        }
        else if (model.Status == RegistrationStatus.Rejected && registration.RejectedAt == null)
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

        var result = new RegistrationManagementUpsertResult
        {
            RegistrationId = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            IsCreated = isCreated,
            Message = isCreated ? "Registration created successfully" : "Registration updated successfully"
        };

        return Result<RegistrationManagementUpsertResult>.Success(result);
    }

    private async Task<string> GenerateRegistrationNumberAsync()
    {
        // Simple implementation - could be more sophisticated
        var year = DateTime.UtcNow.Year;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"REG-{year}-{timestamp}-{random}";
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}