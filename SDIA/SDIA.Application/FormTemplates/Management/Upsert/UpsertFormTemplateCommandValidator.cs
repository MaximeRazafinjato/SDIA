using FluentValidation;

namespace SDIA.Application.FormTemplates.Management.Upsert;

public class UpsertFormTemplateCommandValidator : AbstractValidator<UpsertFormTemplateCommand>
{
    public UpsertFormTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");
            
        RuleFor(x => x.Version)
            .NotEmpty()
            .WithMessage("Version is required")
            .Matches(@"^\d+\.\d+\.\d+$")
            .WithMessage("Version must be in format X.Y.Z (e.g., 1.0.0)");
            
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");
            
        // Validate sections
        RuleForEach(x => x.Sections)
            .SetValidator(new UpsertFormSectionCommandValidator());
        
        // Ensure section orders are unique
        RuleFor(x => x.Sections)
            .Must(sections => sections.Select(s => s.Order).Distinct().Count() == sections.Count())
            .WithMessage("Section orders must be unique")
            .When(x => x.Sections.Any());
    }
}

public class UpsertFormSectionCommandValidator : AbstractValidator<UpsertFormSectionCommand>
{
    public UpsertFormSectionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Section title is required")
            .MaximumLength(200)
            .WithMessage("Section title must not exceed 200 characters");
            
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Section description must not exceed 500 characters");
            
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Section order must be greater than or equal to 0");
            
        // Validate fields
        RuleForEach(x => x.Fields)
            .SetValidator(new UpsertFormFieldCommandValidator());
        
        // Ensure field orders are unique within the section
        RuleFor(x => x.Fields)
            .Must(fields => fields.Select(f => f.Order).Distinct().Count() == fields.Count())
            .WithMessage("Field orders must be unique within the section")
            .When(x => x.Fields.Any());
            
        // Ensure field names are unique within the section
        RuleFor(x => x.Fields)
            .Must(fields => fields.Select(f => f.Name.ToLower()).Distinct().Count() == fields.Count())
            .WithMessage("Field names must be unique within the section")
            .When(x => x.Fields.Any());
    }
}

public class UpsertFormFieldCommandValidator : AbstractValidator<UpsertFormFieldCommand>
{
    private static readonly string[] ValidFieldTypes = {
        "text", "textarea", "email", "password", "number", "tel", "url",
        "date", "datetime-local", "time", "month", "week",
        "checkbox", "radio", "select", "file", "hidden"
    };
    
    public UpsertFormFieldCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Field name is required")
            .MaximumLength(100)
            .WithMessage("Field name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Field name must start with a letter and contain only letters, numbers, and underscores");
            
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Field label is required")
            .MaximumLength(200)
            .WithMessage("Field label must not exceed 200 characters");
            
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Field type is required")
            .Must(type => ValidFieldTypes.Contains(type.ToLower()))
            .WithMessage($"Field type must be one of: {string.Join(", ", ValidFieldTypes)}");
            
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Field order must be greater than or equal to 0");
            
        RuleFor(x => x.Placeholder)
            .MaximumLength(200)
            .WithMessage("Placeholder must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Placeholder));
            
        RuleFor(x => x.ValidationRules)
            .MaximumLength(500)
            .WithMessage("Validation rules must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ValidationRules));
            
        RuleFor(x => x.DefaultValue)
            .MaximumLength(500)
            .WithMessage("Default value must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DefaultValue));
    }
}