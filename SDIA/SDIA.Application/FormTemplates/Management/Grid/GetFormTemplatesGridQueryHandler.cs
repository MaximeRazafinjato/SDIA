using Ardalis.Result;
using MediatR;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.FormTemplates;

namespace SDIA.Application.FormTemplates.Management.Grid;

public class GetFormTemplatesGridQueryHandler : IRequestHandler<GetFormTemplatesGridQuery, Result<GridResult<FormTemplateGridDto>>>
{
    private readonly IFormTemplateRepository _formTemplateRepository;

    public GetFormTemplatesGridQueryHandler(IFormTemplateRepository formTemplateRepository)
    {
        _formTemplateRepository = formTemplateRepository;
    }

    public async Task<Result<GridResult<FormTemplateGridDto>>> Handle(GetFormTemplatesGridQuery request, CancellationToken cancellationToken)
    {
        var query = await _formTemplateRepository.GetQueryableAsync();
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.Name.ToLower().Contains(searchTerm) ||
                x.Description.ToLower().Contains(searchTerm) ||
                x.Version.ToLower().Contains(searchTerm));
        }
        
        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }
        
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == request.OrganizationId.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Version))
        {
            query = query.Where(x => x.Version == request.Version);
        }

        // Map to DTOs
        var mappedQuery = query.Select(x => MapToFormTemplateGridDto(x));
        
        var result = await mappedQuery.ToGridResultAsync(request, cancellationToken);
        
        return Result<GridResult<FormTemplateGridDto>>.Success(result);
    }

    private static FormTemplateGridDto MapToFormTemplateGridDto(FormTemplate formTemplate)
    {
        return new FormTemplateGridDto
        {
            Id = formTemplate.Id,
            Name = formTemplate.Name,
            Description = formTemplate.Description,
            Version = formTemplate.Version,
            IsActive = formTemplate.IsActive,
            OrganizationName = formTemplate.Organization?.Name,
            SectionsCount = formTemplate.Sections.Count,
            FieldsCount = formTemplate.Sections.SelectMany(s => s.Fields).Count(),
            RegistrationsCount = formTemplate.Registrations.Count,
            CreatedAt = formTemplate.CreatedAt,
            UpdatedAt = formTemplate.UpdatedAt
        };
    }
}