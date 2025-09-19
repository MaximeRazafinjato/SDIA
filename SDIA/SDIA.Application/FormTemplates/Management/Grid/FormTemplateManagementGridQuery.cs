using SDIA.Application.Common.Models;

namespace SDIA.Application.FormTemplates.Management.Grid;

public class FormTemplateManagementGridQuery : GridQuery<FormTemplateManagementGridModel>
{
    public bool? IsActive { get; set; }
    public Guid? OrganizationId { get; set; }
}