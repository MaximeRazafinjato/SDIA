using System.Linq.Expressions;
using SDIA.Application.FormTemplates.Management.Grid;
using SDIA.Application.FormTemplates.Management.GetById;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.Common.Mappings;

public static class FormTemplateMappings
{
    public static Expression<Func<FormTemplate, FormTemplateManagementGridModel>> ToGridModel()
    {
        return template => new FormTemplateManagementGridModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Version = template.Version,
            IsActive = template.IsActive,
            OrganizationName = template.Organization != null ? template.Organization.Name : null,
            SectionsCount = template.Sections.Count(),
            FieldsCount = template.Sections.SelectMany(s => s.Fields).Count(),
            RegistrationsCount = template.Registrations.Count(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    public static Expression<Func<FormTemplate, FormTemplateManagementGetByIdModel>> ToDetailModel()
    {
        return template => new FormTemplateManagementGetByIdModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Version = template.Version,
            IsActive = template.IsActive,
            FormSchema = template.FormSchema,
            OrganizationId = template.OrganizationId,
            OrganizationName = template.Organization != null ? template.Organization.Name : null,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}