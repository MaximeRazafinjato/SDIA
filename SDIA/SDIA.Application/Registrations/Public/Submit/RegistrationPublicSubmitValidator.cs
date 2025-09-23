using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.FormTemplates;
using SDIA.Core.Organizations;

namespace SDIA.Application.Registrations.Public.Submit;

public class RegistrationPublicSubmitValidator : IValidator
{
    private readonly IFormTemplateRepository _formTemplateRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public RegistrationPublicSubmitValidator(
        IFormTemplateRepository formTemplateRepository,
        IOrganizationRepository organizationRepository)
    {
        _formTemplateRepository = formTemplateRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken, Guid? id = null)
    {
        if (model is not RegistrationPublicSubmitModel submitModel)
        {
            return Result.Invalid(new List<ValidationError> { new ValidationError { Identifier = "Model", ErrorMessage = "Invalid model type" } });
        }

        var validationErrors = new List<Ardalis.Result.ValidationError>();

        // Validate required fields
        validationErrors.AddErrorIfNullOrWhiteSpace(submitModel.FirstName, nameof(submitModel.FirstName), "First name is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(submitModel.LastName, nameof(submitModel.LastName), "Last name is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(submitModel.Email, nameof(submitModel.Email), "Email is required");
        validationErrors.AddErrorIfNullOrWhiteSpace(submitModel.Phone, nameof(submitModel.Phone), "Phone is required");

        // Validate field lengths
        validationErrors.AddErrorIfExceedsLength(submitModel.FirstName, 100, nameof(submitModel.FirstName), "First name must not exceed 100 characters");
        validationErrors.AddErrorIfExceedsLength(submitModel.LastName, 100, nameof(submitModel.LastName), "Last name must not exceed 100 characters");
        validationErrors.AddErrorIfExceedsLength(submitModel.Email, 255, nameof(submitModel.Email), "Email must not exceed 255 characters");
        validationErrors.AddErrorIfExceedsLength(submitModel.Phone, 20, nameof(submitModel.Phone), "Phone must not exceed 20 characters");

        // Validate email format
        validationErrors.AddErrorIfNotEmail(submitModel.Email, nameof(submitModel.Email), "Invalid email format");

        // Validate birth date
        if (submitModel.BirthDate > DateTime.UtcNow)
        {
            validationErrors.Add(new Ardalis.Result.ValidationError
            {
                Identifier = nameof(submitModel.BirthDate),
                ErrorMessage = "Birth date cannot be in the future"
            });
        }

        var minBirthDate = DateTime.UtcNow.AddYears(-120);
        if (submitModel.BirthDate < minBirthDate)
        {
            validationErrors.Add(new Ardalis.Result.ValidationError
            {
                Identifier = nameof(submitModel.BirthDate),
                ErrorMessage = "Invalid birth date"
            });
        }

        // Validate form template exists and is active
        if (submitModel.FormTemplateId != Guid.Empty)
        {
            var formTemplate = await _formTemplateRepository.GetByIdAsync(submitModel.FormTemplateId, cancellationToken);
            if (formTemplate == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(submitModel.FormTemplateId),
                    ErrorMessage = "Form template not found"
                });
            }
        }

        // Validate organization exists
        if (submitModel.OrganizationId != Guid.Empty)
        {
            var organization = await _organizationRepository.GetByIdAsync(submitModel.OrganizationId, cancellationToken);
            if (organization == null)
            {
                validationErrors.Add(new Ardalis.Result.ValidationError
                {
                    Identifier = nameof(submitModel.OrganizationId),
                    ErrorMessage = "Organization not found"
                });
            }
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}