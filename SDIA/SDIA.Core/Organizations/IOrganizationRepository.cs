using SDIA.SharedKernel.Interfaces;

namespace SDIA.Core.Organizations;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByCodeAsync(string code);
    Task<Organization?> GetByNameAsync(string name);
    Task<IEnumerable<Organization>> GetByTypeAsync(string type);
    Task<bool> ExistsByCodeAsync(string code);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<Organization>> GetWithUsersAsync();
    Task<IEnumerable<Organization>> GetWithFormTemplatesAsync();
    Task<IEnumerable<Organization>> GetWithRegistrationsAsync();
    Task<Organization?> GetWithAllRelationsAsync(Guid id);
}