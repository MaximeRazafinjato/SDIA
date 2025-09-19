namespace SDIA.Application.FormTemplates.Management.Grid;

public class FormTemplateManagementGridModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status => IsActive ? "Active" : "Inactive";
    public string? OrganizationName { get; set; }
    public int SectionsCount { get; set; }
    public int FieldsCount { get; set; }
    public int RegistrationsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}