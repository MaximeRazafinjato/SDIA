namespace SDIA.Application.FormTemplates.Management.GetById;

public class FormTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string FormSchema { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int RegistrationsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<FormSectionDto> Sections { get; set; } = new List<FormSectionDto>();
}

public class FormSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public ICollection<FormFieldDto> Fields { get; set; } = new List<FormFieldDto>();
}

public class FormFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationRules { get; set; }
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }
    public int Order { get; set; }
}