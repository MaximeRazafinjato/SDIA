using Ardalis.Result;
using MediatR;
using SDIA.Core.FormTemplates;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.FormTemplates.Management.Upsert;

public class UpsertFormTemplateCommandHandler : IRequestHandler<UpsertFormTemplateCommand, Result<FormTemplateUpsertResult>>
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public UpsertFormTemplateCommandHandler(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<FormTemplateUpsertResult>> Handle(UpsertFormTemplateCommand request, CancellationToken cancellationToken)
    {
        FormTemplate formTemplate;
        bool isCreated;
        
        if (request.IsUpdate)
        {
            formTemplate = await _formTemplateRepository.GetByIdAsync(request.Id!.Value, cancellationToken);
            if (formTemplate == null)
            {
                return Result<FormTemplateUpsertResult>.NotFound("Form template not found");
            }
            isCreated = false;
        }
        else
        {
            // Check if a form template with the same name exists in the organization
            var existingTemplate = await _formTemplateRepository.GetByNameAndOrganizationAsync(request.Name, request.OrganizationId, cancellationToken);
            if (existingTemplate != null)
            {
                return Result<FormTemplateUpsertResult>.Error("A form template with this name already exists in the organization");
            }
            
            formTemplate = new FormTemplate();
            isCreated = true;
        }

        // Update form template properties
        formTemplate.Name = request.Name;
        formTemplate.Description = request.Description;
        formTemplate.Version = request.Version;
        formTemplate.IsActive = request.IsActive;
        formTemplate.FormSchema = request.FormSchema;
        formTemplate.OrganizationId = request.OrganizationId;

        // Handle sections (simplified - in a real implementation, you'd need more sophisticated handling)
        if (request.Sections.Any())
        {
            // Clear existing sections for updates
            if (!isCreated)
            {
                formTemplate.Sections.Clear();
            }
            
            foreach (var sectionCommand in request.Sections)
            {
                var section = new FormSection
                {
                    Id = sectionCommand.Id ?? Guid.NewGuid(),
                    Name = sectionCommand.Title,
                    Description = sectionCommand.Description,
                    Order = sectionCommand.Order,
                    FormTemplateId = formTemplate.Id
                };
                
                foreach (var fieldCommand in sectionCommand.Fields)
                {
                    if (!Enum.TryParse<FormFieldType>(fieldCommand.Type, true, out var fieldType))
                    {
                        return Result<FormTemplateUpsertResult>.Error($"Invalid field type: {fieldCommand.Type}");
                    }
                    
                    var field = new FormField
                    {
                        Id = fieldCommand.Id ?? Guid.NewGuid(),
                        Name = fieldCommand.Name,
                        Label = fieldCommand.Label,
                        Type = fieldType,
                        IsRequired = fieldCommand.IsRequired,
                        Placeholder = fieldCommand.Placeholder ?? string.Empty,
                        ValidationRules = fieldCommand.ValidationRules ?? string.Empty,
                        DefaultValue = fieldCommand.DefaultValue ?? string.Empty,
                        Options = fieldCommand.Options ?? string.Empty,
                        Order = fieldCommand.Order,
                        FormSectionId = section.Id
                    };
                    section.Fields.Add(field);
                }
                
                formTemplate.Sections.Add(section);
            }
        }

        if (isCreated)
        {
            await _formTemplateRepository.AddAsync(formTemplate, cancellationToken);
        }
        else
        {
            await _formTemplateRepository.UpdateAsync(formTemplate, cancellationToken);
        }

        var result = new FormTemplateUpsertResult
        {
            FormTemplateId = formTemplate.Id,
            Name = formTemplate.Name,
            Version = formTemplate.Version,
            IsCreated = isCreated,
            Message = isCreated ? "Form template created successfully" : "Form template updated successfully"
        };

        return Result<FormTemplateUpsertResult>.Success(result);
    }
}