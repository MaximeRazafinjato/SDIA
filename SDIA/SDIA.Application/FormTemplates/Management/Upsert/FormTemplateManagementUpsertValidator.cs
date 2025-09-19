using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.Upsert;

public class FormTemplateManagementUpsertValidator : IValidator
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementUpsertValidator(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result> ValidateAsync(FormTemplateManagementUpsertModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Check if form template exists when updating
        if (model.IsUpdate)
        {
            if (!model.Id.HasValue)
            {
                return Result.Error("Form template ID is required for update");
            }

            var formTemplate = await _formTemplateRepository.GetByIdAsync(model.Id.Value, cancellationToken);
            if (formTemplate == null)
            {
                return Result.NotFound("Form template not found");
            }
        }

        // Name validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Name, nameof(model.Name), "Form template name is required");
        validationErrors.AddErrorIfExceedsLength(model.Name, 200, nameof(model.Name), "Form template name must not exceed 200 characters");

        // Description validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Description, nameof(model.Description), "Description is required");
        validationErrors.AddErrorIfExceedsLength(model.Description, 1000, nameof(model.Description), "Description must not exceed 1000 characters");

        // Version validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Version, nameof(model.Version), "Version is required");
        validationErrors.AddErrorIfNotMatch(model.Version, @"^\d+\.\d+\.\d+$", nameof(model.Version), "Version must be in format X.Y.Z");

        // OrganizationId validation
        validationErrors.AddErrorIfEmpty(model.OrganizationId, nameof(model.OrganizationId), "Organization is required");

        // Sections validation
        if (model.Sections == null || !model.Sections.Any())
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.Sections), ErrorMessage = "At least one section is required" });
        }
        else
        {
            ValidateSections(model.Sections, validationErrors);
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }

    private static void ValidateSections(IEnumerable<FormTemplateManagementUpsertSectionModel> sections, List<ValidationError> validationErrors)
    {
        int sectionIndex = 0;
        foreach (var section in sections)
        {
            var sectionPrefix = $"Sections[{sectionIndex}]";

            // Section title validation
            validationErrors.AddErrorIfNullOrWhiteSpace(section.Title, $"{sectionPrefix}.Title", "Section title is required");
            validationErrors.AddErrorIfExceedsLength(section.Title, 200, $"{sectionPrefix}.Title", "Section title must not exceed 200 characters");

            // Section description validation
            validationErrors.AddErrorIfExceedsLength(section.Description, 500, $"{sectionPrefix}.Description", "Section description must not exceed 500 characters");

            // Section order validation
            validationErrors.AddErrorIfLessThan(section.Order, 0, $"{sectionPrefix}.Order", "Section order must be non-negative");

            // Fields validation
            if (section.Fields == null || !section.Fields.Any())
            {
                validationErrors.Add(new ValidationError { Identifier = $"{sectionPrefix}.Fields", ErrorMessage = "At least one field is required in each section" });
            }
            else
            {
                ValidateFields(section.Fields, validationErrors, sectionPrefix);
            }

            sectionIndex++;
        }
    }

    private static void ValidateFields(IEnumerable<FormTemplateManagementUpsertFieldModel> fields, List<ValidationError> validationErrors, string sectionPrefix)
    {
        var validFieldTypes = new[] { "text", "email", "number", "date", "datetime", "select", "multiselect", "checkbox", "radio", "textarea", "file" };

        int fieldIndex = 0;
        foreach (var field in fields)
        {
            var fieldPrefix = $"{sectionPrefix}.Fields[{fieldIndex}]";

            // Field name validation
            validationErrors.AddErrorIfNullOrWhiteSpace(field.Name, $"{fieldPrefix}.Name", "Field name is required");
            if (!string.IsNullOrWhiteSpace(field.Name))
            {
                validationErrors.AddErrorIfExceedsLength(field.Name, 100, $"{fieldPrefix}.Name", "Field name must not exceed 100 characters");
                validationErrors.AddErrorIfNotMatch(field.Name, @"^[a-zA-Z][a-zA-Z0-9_]*$", $"{fieldPrefix}.Name",
                    "Field name must start with a letter and contain only letters, numbers, and underscores");
            }

            // Field label validation
            validationErrors.AddErrorIfNullOrWhiteSpace(field.Label, $"{fieldPrefix}.Label", "Field label is required");
            validationErrors.AddErrorIfExceedsLength(field.Label, 200, $"{fieldPrefix}.Label", "Field label must not exceed 200 characters");

            // Field type validation
            validationErrors.AddErrorIfNullOrWhiteSpace(field.Type, $"{fieldPrefix}.Type", "Field type is required");
            if (!string.IsNullOrWhiteSpace(field.Type) && !validFieldTypes.Contains(field.Type))
            {
                validationErrors.Add(new ValidationError {
                    Identifier = $"{fieldPrefix}.Type",
                    ErrorMessage = $"Field type must be one of: {string.Join(", ", validFieldTypes)}"
                });
            }

            // Field order validation
            validationErrors.AddErrorIfLessThan(field.Order, 0, $"{fieldPrefix}.Order", "Field order must be non-negative");

            // Options validation for select/multiselect/radio fields
            if ((field.Type == "select" || field.Type == "multiselect" || field.Type == "radio") && string.IsNullOrWhiteSpace(field.Options))
            {
                validationErrors.Add(new ValidationError {
                    Identifier = $"{fieldPrefix}.Options",
                    ErrorMessage = "Options are required for select, multiselect, and radio fields"
                });
            }

            fieldIndex++;
        }
    }
}