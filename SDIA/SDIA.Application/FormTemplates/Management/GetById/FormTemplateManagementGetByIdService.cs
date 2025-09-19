using Ardalis.Result;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.GetById;

public class FormTemplateManagementGetByIdService
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementGetByIdService(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<FormTemplateManagementGetByIdModel>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var formTemplate = await _formTemplateRepository.GetByIdWithIncludeAsync(id, cancellationToken,
            ft => ft.Organization,
            ft => ft.Sections,
            ft => ft.Sections.SelectMany(s => s.Fields));

        if (formTemplate == null)
        {
            return Result<FormTemplateManagementGetByIdModel>.NotFound("Form template not found");
        }

        var model = MapToModel(formTemplate);
        return Result<FormTemplateManagementGetByIdModel>.Success(model);
    }

    private static FormTemplateManagementGetByIdModel MapToModel(FormTemplate formTemplate)
    {
        return new FormTemplateManagementGetByIdModel
        {
            Id = formTemplate.Id,
            Name = formTemplate.Name,
            Description = formTemplate.Description,
            Version = formTemplate.Version,
            IsActive = formTemplate.IsActive,
            FormSchema = formTemplate.FormSchema,
            OrganizationId = formTemplate.OrganizationId,
            OrganizationName = formTemplate.Organization?.Name,
            CreatedAt = formTemplate.CreatedAt,
            UpdatedAt = formTemplate.UpdatedAt,
            Sections = formTemplate.Sections?.Select(MapToSectionModel).ToList() ?? new List<FormSectionModel>()
        };
    }

    private static FormSectionModel MapToSectionModel(FormSection section)
    {
        return new FormSectionModel
        {
            Id = section.Id,
            Title = section.Name,
            Description = section.Description,
            Order = section.Order,
            Fields = section.Fields?.Select(MapToFieldModel).ToList() ?? new List<FormFieldModel>()
        };
    }

    private static FormFieldModel MapToFieldModel(FormField field)
    {
        return new FormFieldModel
        {
            Id = field.Id,
            Name = field.Name,
            Label = field.Label,
            Type = field.Type.ToString(),
            IsRequired = field.IsRequired,
            Placeholder = field.Placeholder,
            ValidationRules = field.ValidationRules,
            DefaultValue = field.DefaultValue,
            Options = field.Options,
            Order = field.Order
        };
    }
}