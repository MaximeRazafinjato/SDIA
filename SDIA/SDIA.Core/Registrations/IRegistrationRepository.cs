using SDIA.SharedKernel.Interfaces;

namespace SDIA.Core.Registrations;

public interface IRegistrationRepository : IRepository<Registration>
{
    Task<IEnumerable<Registration>> GetByOrganizationIdAsync(Guid organizationId);
    Task<IEnumerable<Registration>> GetByFormTemplateIdAsync(Guid formTemplateId);
    Task<IEnumerable<Registration>> GetByStatusAsync(string status);
    Task<IEnumerable<Registration>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Registration>> GetByManagedByUserIdAsync(Guid userId);
    Task<Registration?> GetByEmailAsync(string email);
    Task<Registration?> GetByTokenAsync(string token);
    Task<Registration?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<Registration>> GetWithDocumentsAsync();
    Task<IEnumerable<Registration>> GetPendingRegistrationsAsync();
    Task<IEnumerable<Registration>> GetCompletedRegistrationsAsync();
    Task<int> GetCountByStatusAsync(string status);
    Task<IEnumerable<Registration>> GetRecentRegistrationsAsync(int count);
}