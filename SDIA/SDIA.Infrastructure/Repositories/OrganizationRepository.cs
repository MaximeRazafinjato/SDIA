using Microsoft.EntityFrameworkCore;
using SDIA.Core.Organizations;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(SDIADbContext context) : base(context)
    {
    }

    public async Task<Organization?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(o => o.Code == code);
    }

    public async Task<Organization?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(o => o.Name == name);
    }

    public async Task<IEnumerable<Organization>> GetByTypeAsync(string type)
    {
        return await _dbSet
            .Where(o => o.Type == type)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbSet.AnyAsync(o => o.Code == code);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(o => o.Name == name);
    }

    public async Task<IEnumerable<Organization>> GetWithUsersAsync()
    {
        return await _dbSet
            .Include(o => o.Users)
            .ToListAsync();
    }

    public async Task<IEnumerable<Organization>> GetWithFormTemplatesAsync()
    {
        return await _dbSet
            .Include(o => o.FormTemplates)
            .ToListAsync();
    }

    public async Task<IEnumerable<Organization>> GetWithRegistrationsAsync()
    {
        return await _dbSet
            .Include(o => o.Registrations)
            .ToListAsync();
    }

    public async Task<Organization?> GetWithAllRelationsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Users)
            .Include(o => o.FormTemplates)
            .Include(o => o.Registrations)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}