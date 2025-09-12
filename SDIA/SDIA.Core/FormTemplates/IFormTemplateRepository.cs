using SDIA.SharedKernel.Interfaces;

namespace SDIA.Core.FormTemplates;

public interface IFormTemplateRepository : IRepository<FormTemplate>
{
    Task<IEnumerable<FormTemplate>> GetByOrganizationIdAsync(Guid organizationId);
    Task<FormTemplate?> GetByNameAsync(string name);
    Task<IEnumerable<FormTemplate>> GetActiveFormTemplatesAsync();
    Task<IEnumerable<FormTemplate>> GetByTypeAsync(string type);
    Task<IEnumerable<FormTemplate>> GetWithRegistrationsAsync();
    Task<FormTemplate?> GetWithRegistrationsAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
    Task<IEnumerable<FormTemplate>> GetPublishedFormTemplatesAsync();
    Task<IEnumerable<FormTemplate>> GetDraftFormTemplatesAsync();
    Task<int> GetRegistrationCountAsync(Guid formTemplateId);
    Task<FormTemplate?> GetByNameAndOrganizationAsync(string name, Guid organizationId, CancellationToken cancellationToken = default);
}