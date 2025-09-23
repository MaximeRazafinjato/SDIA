using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.GetAll;

public class FormTemplateManagementGetAllService
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public FormTemplateManagementGetAllService(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<GridResult<FormTemplateManagementGetAllModel>>> ExecuteAsync(
        FormTemplateManagementGetAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _formTemplateRepository.GetAll(cancellationToken);

        // Apply text search
        dbQuery = dbQuery.ApplyTextSearch(query.SearchTerm,
            ft => ft.Name,
            ft => ft.Description,
            ft => ft.Version);

        // Apply filters
        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(ft => ft.IsActive == query.IsActive.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(ft => ft.OrganizationId == query.OrganizationId.Value);
        }

        // Get total count first
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Apply sorting and paging
        var pagedQuery = dbQuery
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Execute query
        var items = await pagedQuery.ToListAsync(cancellationToken);

        // Map to model
        var mappedItems = items.Select(ft => new FormTemplateManagementGetAllModel
        {
            Id = ft.Id,
            Name = ft.Name,
            Description = ft.Description ?? string.Empty,
            Type = "Standard",
            Status = ft.IsActive ? "Active" : "Inactive",
            IsActive = ft.IsActive,
            OrganizationName = ft.Organization?.Name,
            RegistrationCount = ft.Registrations?.Count ?? 0,
            CreatedAt = ft.CreatedAt,
            UpdatedAt = ft.UpdatedAt ?? ft.CreatedAt
        }).ToList();

        var result = new GridResult<FormTemplateManagementGetAllModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<FormTemplateManagementGetAllModel>>.Success(result);
    }
}