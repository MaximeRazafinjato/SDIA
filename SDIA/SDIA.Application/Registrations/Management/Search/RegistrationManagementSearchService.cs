using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Search;

public class RegistrationManagementSearchService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementSearchService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<IEnumerable<RegistrationManagementSearchResult>>> ExecuteAsync(
        RegistrationManagementSearchModel model,
        CancellationToken cancellationToken = default)
    {
        var query = _registrationRepository.GetAll(cancellationToken);

        // Apply search term if provided
        if (!string.IsNullOrWhiteSpace(model.SearchTerm))
        {
            var searchTerm = model.SearchTerm.ToLower();
            query = query.Where(r =>
                r.RegistrationNumber.ToLower().Contains(searchTerm) ||
                r.FirstName.ToLower().Contains(searchTerm) ||
                r.LastName.ToLower().Contains(searchTerm) ||
                r.Email.ToLower().Contains(searchTerm) ||
                r.Phone.ToLower().Contains(searchTerm));
        }

        // Apply organization filter if provided
        if (model.OrganizationId.HasValue)
        {
            query = query.Where(r => r.OrganizationId == model.OrganizationId.Value);
        }

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(model.Status) && Enum.TryParse<RegistrationStatus>(model.Status, out var statusEnum))
        {
            query = query.Where(r => r.Status == statusEnum);
        }

        // Order by creation date (most recent first) and limit results
        query = query
            .OrderByDescending(r => r.CreatedAt)
            .Take(model.MaxResults);

        // Execute query
        var registrations = await Task.Run(() => query.ToList(), cancellationToken);

        // Map to result models
        var results = registrations.Select(MapToResult);

        return Result<IEnumerable<RegistrationManagementSearchResult>>.Success(results);
    }

    private static RegistrationManagementSearchResult MapToResult(Registration registration)
    {
        return new RegistrationManagementSearchResult
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            Phone = registration.Phone,
            Status = registration.Status.ToString(),
            FormTemplateName = registration.FormTemplate?.Name,
            CreatedAt = registration.CreatedAt
        };
    }
}