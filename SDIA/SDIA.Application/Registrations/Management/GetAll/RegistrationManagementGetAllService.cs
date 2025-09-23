using Ardalis.Result;
using SDIA.Application.Common.Extensions;
using SDIA.Application.Common.Models;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.GetAll;

public class RegistrationManagementGetAllService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementGetAllService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<GridResult<RegistrationManagementGetAllModel>>> ExecuteAsync(
        RegistrationManagementGetAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _registrationRepository.GetAll(cancellationToken);

        // Apply text search
        dbQuery = dbQuery.ApplyTextSearch(query.SearchTerm,
            r => r.RegistrationNumber,
            r => r.FirstName,
            r => r.LastName,
            r => r.Email,
            r => r.Phone);

        // Apply status filters
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<RegistrationStatus>(query.Status, out var statusEnum))
        {
            dbQuery = dbQuery.Where(r => r.Status == statusEnum);
        }

        if (query.EmailVerified.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.EmailVerified == query.EmailVerified.Value);
        }

        if (query.PhoneVerified.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.PhoneVerified == query.PhoneVerified.Value);
        }

        if (query.IsMinor.HasValue)
        {
            var eighteenYearsAgo = DateTime.UtcNow.AddYears(-18);
            if (query.IsMinor.Value)
            {
                dbQuery = dbQuery.Where(r => r.BirthDate > eighteenYearsAgo);
            }
            else
            {
                dbQuery = dbQuery.Where(r => r.BirthDate <= eighteenYearsAgo);
            }
        }

        // Apply organization and assignment filters
        if (query.OrganizationId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.OrganizationId == query.OrganizationId.Value);
        }

        if (query.FormTemplateId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.FormTemplateId == query.FormTemplateId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.IsAssigned.HasValue)
        {
            if (query.IsAssigned.Value)
            {
                dbQuery = dbQuery.Where(r => r.AssignedToUserId != null);
            }
            else
            {
                dbQuery = dbQuery.Where(r => r.AssignedToUserId == null);
            }
        }

        // Apply date range filters
        if (query.SubmittedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.SubmittedAt >= query.SubmittedFrom.Value);
        }

        if (query.SubmittedTo.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.SubmittedAt <= query.SubmittedTo.Value);
        }

        if (query.ValidatedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.ValidatedAt >= query.ValidatedFrom.Value);
        }

        if (query.ValidatedTo.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.ValidatedAt <= query.ValidatedTo.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.CreatedAt <= query.CreatedTo.Value);
        }

        if (query.BirthDateFrom.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.BirthDate >= query.BirthDateFrom.Value);
        }

        if (query.BirthDateTo.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.BirthDate <= query.BirthDateTo.Value);
        }

        // Get total count first
        var totalCount = await Task.Run(() => dbQuery.Count(), cancellationToken);

        // Apply sorting and paging
        var pagedQuery = dbQuery
            .ApplySorting(query.SortBy, query.SortDescending)
            .ApplyPaging(query.Page, query.PageSize);

        // Materialize the results
        var entities = await Task.Run(() => pagedQuery.ToList(), cancellationToken);

        // Map to models
        var mappedItems = entities.Select(MapToModel).ToList();

        var result = new GridResult<RegistrationManagementGetAllModel>
        {
            Data = mappedItems,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Result<GridResult<RegistrationManagementGetAllModel>>.Success(result);
    }

    private static RegistrationManagementGetAllModel MapToModel(Registration registration)
    {
        return new RegistrationManagementGetAllModel
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status.ToString(),
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            BirthDate = registration.BirthDate,
            IsMinor = registration.IsMinor,
            SubmittedAt = registration.SubmittedAt,
            ValidatedAt = registration.ValidatedAt,
            RejectedAt = registration.RejectedAt,
            RejectionReason = registration.RejectionReason,
            FormTemplateName = registration.FormTemplate?.Name,
            OrganizationName = registration.Organization?.Name,
            AssignedToUserName = registration.AssignedToUser != null
                ? $"{registration.AssignedToUser.FirstName} {registration.AssignedToUser.LastName}"
                : null,
            EmailVerified = registration.EmailVerified,
            PhoneVerified = registration.PhoneVerified,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt ?? registration.CreatedAt
        };
    }
}