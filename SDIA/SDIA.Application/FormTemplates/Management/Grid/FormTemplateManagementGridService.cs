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

        // Apply text search using the new extension
        dbQuery = dbQuery.ApplyTextSearch(query.SearchTerm,
            x => x.Name,
            x => x.Description,
            x => x.Version);

        // Apply specific filters
        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.OrganizationId == query.OrganizationId.Value);
        }

        // Get total count first
        var totalCount = await Task.Run(() => dbQuery.Count(), cancellationToken);

        // Apply sorting and paging on the entity query
        var pagedQuery = dbQuery
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Materialize the results
        var entities = await Task.Run(() => pagedQuery.ToList(), cancellationToken);

        // Map to models in memory
        var mappedItems = entities.Select(x => MapToGridModel(x)).ToList();

        var result = new GridResult<FormTemplateManagementGridModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

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