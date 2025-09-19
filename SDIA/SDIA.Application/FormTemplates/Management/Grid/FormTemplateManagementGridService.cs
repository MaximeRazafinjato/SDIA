using Ardalis.Result;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.Grid;

public class FormTemplateManagementGridService
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementGridService(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<GridResult<FormTemplateManagementGridModel>>> ExecuteAsync(
        FormTemplateManagementGridQuery query,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = await _formTemplateRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(x =>
                x.Name.ToLower().Contains(searchTerm) ||
                x.Description.ToLower().Contains(searchTerm) ||
                x.Version.ToLower().Contains(searchTerm));
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.OrganizationId == query.OrganizationId.Value);
        }

        // Map to models
        var mappedQuery = dbQuery.Select(x => MapToGridModel(x));

        var result = await mappedQuery.ToGridResultAsync(query, cancellationToken);

        return Result<GridResult<FormTemplateManagementGridModel>>.Success(result);
    }

    private static FormTemplateManagementGridModel MapToGridModel(FormTemplate formTemplate)
    {
        return new FormTemplateManagementGridModel
        {
            Id = formTemplate.Id,
            Name = formTemplate.Name,
            Description = formTemplate.Description,
            Version = formTemplate.Version,
            IsActive = formTemplate.IsActive,
            OrganizationName = null, // Organization relation is not included in the query
            SectionsCount = 0, // Sections relation is not included in the query
            FieldsCount = 0, // Sections relation is not included in the query
            RegistrationsCount = 0, // Registrations relation is not included in the query
            CreatedAt = formTemplate.CreatedAt,
            UpdatedAt = formTemplate.UpdatedAt
        };
    }
}