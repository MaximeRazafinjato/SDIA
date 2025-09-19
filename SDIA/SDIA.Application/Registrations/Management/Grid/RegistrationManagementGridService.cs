using Ardalis.Result;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.Grid;

public class RegistrationManagementGridService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementGridService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<GridResult<RegistrationManagementGridModel>>> ExecuteAsync(RegistrationManagementGridQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = await _registrationRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(x =>
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                x.Phone.Contains(searchTerm) ||
                x.RegistrationNumber.ToLower().Contains(searchTerm));
        }

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.OrganizationId == query.OrganizationId.Value);
        }

        if (query.FormTemplateId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.FormTemplateId == query.FormTemplateId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.SubmittedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SubmittedAt >= query.SubmittedFrom.Value);
        }

        if (query.SubmittedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SubmittedAt <= query.SubmittedTo.Value);
        }

        if (query.EmailVerified.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.EmailVerified == query.EmailVerified.Value);
        }

        if (query.PhoneVerified.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.PhoneVerified == query.PhoneVerified.Value);
        }

        // First get total count from the database query
        var totalCount = await Task.Run(() => dbQuery.Count(), cancellationToken);

        // Apply paging and sorting, then materialize
        var items = await Task.Run(() => dbQuery
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize)
            .ToList(), cancellationToken);

        // Map to models in memory
        var mappedItems = items.Select(x => MapToGridModel(x)).ToList();

        var result = new GridResult<RegistrationManagementGridModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<RegistrationManagementGridModel>>.Success(result);
    }

    private static RegistrationManagementGridModel MapToGridModel(Registration registration)
    {
        return new RegistrationManagementGridModel
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            IsMinor = registration.IsMinor,
            SubmittedAt = registration.SubmittedAt,
            EmailVerified = registration.EmailVerified,
            PhoneVerified = registration.PhoneVerified,
            OrganizationName = null, // Organization relation is not included in the query
            FormTemplateName = null, // FormTemplate relation is not included in the query
            AssignedToUserName = null, // AssignedToUser relation is not included in the query
            CreatedAt = registration.CreatedAt
        };
    }
}