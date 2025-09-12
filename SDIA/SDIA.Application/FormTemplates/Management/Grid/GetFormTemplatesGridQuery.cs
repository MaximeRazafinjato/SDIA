using SDIA.Application.Common.Models;

namespace SDIA.Application.FormTemplates.Management.Grid;

public class GetFormTemplatesGridQuery : GridQuery<FormTemplateGridDto>
{
    public bool? IsActive { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? Version { get; set; }
}