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

        // Apply text search using the new extension
        dbQuery = dbQuery.ApplyTextSearch(query.SearchTerm,
            x => x.FirstName,
            x => x.LastName,
            x => x.Email,
            x => x.Phone,
            x => x.RegistrationNumber);

        // Apply specific filters
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

        // Submission date filters
        if (query.SubmittedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SubmittedAt >= query.SubmittedFrom.Value);
        }

        if (query.SubmittedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SubmittedAt <= query.SubmittedTo.Value);
        }

        // Creation date filters
        if (query.CreatedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        // Validation date filters
        if (query.ValidatedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.ValidatedAt >= query.ValidatedFrom.Value);
        }

        if (query.ValidatedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.ValidatedAt <= query.ValidatedTo.Value);
        }

        // Rejection date filters
        if (query.RejectedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.RejectedAt >= query.RejectedFrom.Value);
        }

        if (query.RejectedTo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.RejectedAt <= query.RejectedTo.Value);
        }

        // Verification filters
        if (query.EmailVerified.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.EmailVerified == query.EmailVerified.Value);
        }

        if (query.PhoneVerified.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.PhoneVerified == query.PhoneVerified.Value);
        }

        // Minor status filter
        if (query.IsMinor.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsMinor == query.IsMinor.Value);
        }

        // Age range filters (only apply if BirthDate is provided)
        if (query.AgeFrom.HasValue)
        {
            var maxBirthDate = DateTime.Today.AddYears(-query.AgeFrom.Value);
            dbQuery = dbQuery.Where(x => x.BirthDate <= maxBirthDate);
        }

        if (query.AgeTo.HasValue)
        {
            var minBirthDate = DateTime.Today.AddYears(-query.AgeTo.Value - 1);
            dbQuery = dbQuery.Where(x => x.BirthDate >= minBirthDate);
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