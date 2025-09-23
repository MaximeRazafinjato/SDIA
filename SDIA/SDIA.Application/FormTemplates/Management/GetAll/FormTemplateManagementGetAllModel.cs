namespace SDIA.Application.FormTemplates.Management.GetAll;

public class FormTemplateManagementGetAllModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? OrganizationName { get; set; }
    public int RegistrationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FormTemplateManagementGetAllQuery
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public Guid? OrganizationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}