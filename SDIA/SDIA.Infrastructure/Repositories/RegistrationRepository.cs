using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class RegistrationRepository : BaseRepository<Registration>, IRegistrationRepository
{
    public RegistrationRepository(SDIADbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Registration>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(r => r.OrganizationId == organizationId)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .Include(r => r.ManagedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetByFormTemplateIdAsync(Guid formTemplateId)
    {
        return await _dbSet
            .Where(r => r.FormTemplateId == formTemplateId)
            .Include(r => r.FormTemplate)
            .Include(r => r.Organization)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(r => r.Status == status)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetByManagedByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(r => r.ManagedById == userId)
            .Include(r => r.ManagedBy)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .ToListAsync();
    }

    public async Task<Registration?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(r => r.Email == email)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .FirstOrDefaultAsync();
    }

    public async Task<Registration?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Where(r => r.Token == token)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Registration>> GetWithDocumentsAsync()
    {
        return await _dbSet
            .Include(r => r.Documents)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetPendingRegistrationsAsync()
    {
        return await _dbSet
            .Where(r => r.Status == "Pending" || r.Status == "InProgress")
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetCompletedRegistrationsAsync()
    {
        return await _dbSet
            .Where(r => r.Status == "Completed" || r.Status == "Approved")
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        return await _dbSet.CountAsync(r => r.Status == status);
    }

    public async Task<IEnumerable<Registration>> GetRecentRegistrationsAsync(int count)
    {
        return await _dbSet
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}