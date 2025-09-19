using Ardalis.Result;
using SDIA.Core.FormTemplates;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.FormTemplates.Management.Upsert;

public class FormTemplateManagementUpsertService
{
    private readonly FormTemplateManagementUpsertValidator _validator;
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementUpsertService(
        FormTemplateManagementUpsertValidator validator,
        IFormTemplateRepository formTemplateRepository)
    {
        _validator = validator;
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<FormTemplateManagementUpsertResult>> ExecuteAsync(
        FormTemplateManagementUpsertModel model,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<FormTemplateManagementUpsertResult>.Invalid(validationResult.ValidationErrors);
        }

        FormTemplate formTemplate;
        bool isCreated;

        if (model.IsUpdate)
        {
            formTemplate = await _formTemplateRepository.GetByIdWithIncludeAsync(model.Id!.Value, cancellationToken,
                ft => ft.Sections,
                ft => ft.Sections.SelectMany(s => s.Fields));
            if (formTemplate == null)
            {
                return Result<FormTemplateManagementUpsertResult>.NotFound("Form template not found");
            }
            isCreated = false;

            // Clear existing sections for update
            formTemplate.Sections.Clear();
        }
        else
        {
            formTemplate = new FormTemplate();
            isCreated = true;
        }

        // Update form template properties
        formTemplate.Name = model.Name;
        formTemplate.Description = model.Description;
        formTemplate.Version = model.Version;
        formTemplate.IsActive = model.IsActive;
        formTemplate.FormSchema = model.FormSchema;
        formTemplate.OrganizationId = model.OrganizationId;

        // Update sections
        foreach (var sectionModel in model.Sections.OrderBy(s => s.Order))
        {
            var section = new FormSection
            {
                Id = sectionModel.Id ?? Guid.NewGuid(),
                Name = sectionModel.Title,
                Description = sectionModel.Description,
                Order = sectionModel.Order,
                FormTemplateId = formTemplate.Id,
                Fields = new List<FormField>()
            };

            foreach (var fieldModel in sectionModel.Fields.OrderBy(f => f.Order))
            {
                var field = new FormField
                {
                    Id = fieldModel.Id ?? Guid.NewGuid(),
                    Name = fieldModel.Name,
                    Label = fieldModel.Label,
                    Type = Enum.Parse<FormFieldType>(fieldModel.Type),
                    IsRequired = fieldModel.IsRequired,
                    Placeholder = fieldModel.Placeholder ?? string.Empty,
                    ValidationRules = fieldModel.ValidationRules ?? string.Empty,
                    DefaultValue = fieldModel.DefaultValue ?? string.Empty,
                    Options = fieldModel.Options ?? string.Empty,
                    Order = fieldModel.Order,
                    FormSectionId = section.Id
                };
                section.Fields.Add(field);
            }

            formTemplate.Sections.Add(section);
        }

        if (isCreated)
        {
            await _formTemplateRepository.AddAsync(formTemplate, cancellationToken);
        }
        else
        {
            await _formTemplateRepository.UpdateAsync(formTemplate, cancellationToken);
        }

        var result = new FormTemplateManagementUpsertResult
        {
            FormTemplateId = formTemplate.Id,
            IsCreated = isCreated,
            Message = isCreated ? "Form template created successfully" : "Form template updated successfully"
        };

        return Result<FormTemplateManagementUpsertResult>.Success(result);
    }
}