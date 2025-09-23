using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Statistics;

public class RegistrationManagementStatisticsService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementStatisticsService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementStatisticsModel>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var query = _registrationRepository.GetAll(cancellationToken);

        var statistics = await query
            .GroupBy(r => r.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalRegistrations = await query.CountAsync(cancellationToken);

        var result = new RegistrationManagementStatisticsModel
        {
            TotalRegistrations = totalRegistrations,
            DraftCount = statistics.FirstOrDefault(s => s.Status == RegistrationStatus.Draft)?.Count ?? 0,
            PendingCount = statistics.FirstOrDefault(s => s.Status == RegistrationStatus.Pending)?.Count ?? 0,
            ValidatedCount = statistics.FirstOrDefault(s => s.Status == RegistrationStatus.Validated)?.Count ?? 0,
            RejectedCount = statistics.FirstOrDefault(s => s.Status == RegistrationStatus.Rejected)?.Count ?? 0,
            NewRegistrationsToday = await query
                .Where(r => r.CreatedAt.Date == DateTime.UtcNow.Date)
                .CountAsync(cancellationToken),
            PendingReviewCount = await query
                .Where(r => r.Status == RegistrationStatus.Pending)
                .CountAsync(cancellationToken)
        };

        return Result<RegistrationManagementStatisticsModel>.Success(result);
    }
}

public class RegistrationManagementStatisticsModel
{
    public int TotalRegistrations { get; set; }
    public int DraftCount { get; set; }
    public int PendingCount { get; set; }
    public int ValidatedCount { get; set; }
    public int RejectedCount { get; set; }
    public int NewRegistrationsToday { get; set; }
    public int PendingReviewCount { get; set; }
}