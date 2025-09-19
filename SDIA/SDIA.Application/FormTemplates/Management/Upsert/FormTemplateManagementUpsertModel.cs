namespace SDIA.Application.FormTemplates.Management.Upsert;

public class FormTemplateManagementUpsertModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public bool IsActive { get; set; } = true;
    public string FormSchema { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public ICollection<FormTemplateManagementUpsertSectionModel> Sections { get; set; } = new List<FormTemplateManagementUpsertSectionModel>();

    public bool IsUpdate => Id.HasValue && Id.Value != Guid.Empty;
}

public class FormTemplateManagementUpsertSectionModel
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public ICollection<FormTemplateManagementUpsertFieldModel> Fields { get; set; } = new List<FormTemplateManagementUpsertFieldModel>();
}

public class FormTemplateManagementUpsertFieldModel
{
    public Guid? Id { get; set; }
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