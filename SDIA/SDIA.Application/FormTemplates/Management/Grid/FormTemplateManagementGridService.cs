using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Mappings;
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
        var dbQuery = _formTemplateRepository.GetAll(cancellationToken);

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
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Apply projection, sorting and paging
        var mappedQuery = dbQuery
            .Select(FormTemplateMappings.ToGridModel())
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Execute query with projection
        var mappedItems = await mappedQuery.ToListAsync(cancellationToken);

        var result = new GridResult<FormTemplateManagementGridModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<FormTemplateManagementGridModel>>.Success(result);
    }
}