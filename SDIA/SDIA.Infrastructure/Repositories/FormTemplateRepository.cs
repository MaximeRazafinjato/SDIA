using Microsoft.EntityFrameworkCore;
using SDIA.Core.FormTemplates;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class FormTemplateRepository : BaseRepository<FormTemplate>, IFormTemplateRepository
{
    public FormTemplateRepository(SDIADbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FormTemplate>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(ft => ft.OrganizationId == organizationId)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .ToListAsync();
    }

    public async Task<FormTemplate?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Where(ft => ft.Name == name)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .FirstOrDefaultAsync();
    }

    public async Task<FormTemplate?> GetByNameAndOrganizationAsync(string name, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ft => ft.Name == name && ft.OrganizationId == organizationId)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FormTemplate>> GetActiveFormTemplatesAsync()
    {
        return await _dbSet
            .Where(ft => ft.IsActive)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormTemplate>> GetByTypeAsync(string type)
    {
        // Type property doesn't exist, return active templates for now
        return await _dbSet
            .Where(ft => ft.IsActive)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormTemplate>> GetWithRegistrationsAsync()
    {
        return await _dbSet
            .Include(ft => ft.Registrations)
            .Include(ft => ft.Organization)
            .ToListAsync();
    }

    public async Task<FormTemplate?> GetWithRegistrationsAsync(Guid id)
    {
        return await _dbSet
            .Where(ft => ft.Id == id)
            .Include(ft => ft.Registrations)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(ft => ft.Name == name);
    }

    public async Task<IEnumerable<FormTemplate>> GetPublishedFormTemplatesAsync()
    {
        return await _dbSet
            .Where(ft => ft.IsActive)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .ToListAsync();
    }

    public async Task<IEnumerable<FormTemplate>> GetDraftFormTemplatesAsync()
    {
        return await _dbSet
            .Where(ft => !ft.IsActive)
            .Include(ft => ft.Organization)
            .Include(ft => ft.Sections)
                .ThenInclude(s => s.Fields)
            .ToListAsync();
    }

    public async Task<int> GetRegistrationCountAsync(Guid formTemplateId)
    {
        return await _context.Registrations
            .CountAsync(r => r.FormTemplateId == formTemplateId);
    }
}