using Ardalis.Result;
using MediatR;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.GetById;

public class GetFormTemplateByIdQueryHandler : IRequestHandler<GetFormTemplateByIdQuery, Result<FormTemplateDto>>
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public GetFormTemplateByIdQueryHandler(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<FormTemplateDto>> Handle(GetFormTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var formTemplate = await _formTemplateRepository.GetByIdAsync(request.FormTemplateId, cancellationToken);
        
        if (formTemplate == null)
        {
            return Result<FormTemplateDto>.NotFound("Form template not found");
        }

        var formTemplateDto = MapToFormTemplateDto(formTemplate);
        return Result<FormTemplateDto>.Success(formTemplateDto);
    }

    private static FormTemplateDto MapToFormTemplateDto(FormTemplate formTemplate)
    {
        return new FormTemplateDto
        {
            Id = formTemplate.Id,
            Name = formTemplate.Name,
            Description = formTemplate.Description,
            Version = formTemplate.Version,
            IsActive = formTemplate.IsActive,
            FormSchema = formTemplate.FormSchema,
            OrganizationId = formTemplate.OrganizationId,
            OrganizationName = formTemplate.Organization?.Name,
            RegistrationsCount = formTemplate.Registrations.Count,
            CreatedAt = formTemplate.CreatedAt,
            UpdatedAt = formTemplate.UpdatedAt,
            Sections = formTemplate.Sections.Select(MapToFormSectionDto).ToList()
        };
    }

    private static FormSectionDto MapToFormSectionDto(FormSection section)
    {
        return new FormSectionDto
        {
            Id = section.Id,
            Title = section.Name,
            Description = section.Description,
            Order = section.Order,
            Fields = section.Fields.Select(MapToFormFieldDto).ToList()
        };
    }

    private static FormFieldDto MapToFormFieldDto(FormField field)
    {
        return new FormFieldDto
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