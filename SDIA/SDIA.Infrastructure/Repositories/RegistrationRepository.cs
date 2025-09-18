using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.Infrastructure.Data;
using SDIA.SharedKernel.Enums;

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
            // .Include(r => r.ManagedBy) - Property doesn't exist yet
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
        // Parse status string to enum
        if (!Enum.TryParse<RegistrationStatus>(status, out var statusEnum))
        {
            return new List<Registration>();
        }

        return await _dbSet
            .Where(r => r.Status == statusEnum)
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
            // ManagedById property doesn't exist yet
            .Where(r => false) // Placeholder - will return empty
            // .Include(r => r.ManagedBy) - Property doesn't exist yet
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
            // Token property doesn't exist, using AccessToken instead
            .Where(r => r.AccessToken == token)
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
            .Where(r => r.Status == RegistrationStatus.Pending || r.Status == RegistrationStatus.InProgress)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Registration>> GetCompletedRegistrationsAsync()
    {
        return await _dbSet
            .Where(r => r.Status == RegistrationStatus.Completed || r.Status == RegistrationStatus.Validated)
            .Include(r => r.Organization)
            .Include(r => r.FormTemplate)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        // Parse status string to enum
        if (!Enum.TryParse<RegistrationStatus>(status, out var statusEnum))
        {
            return 0;
        }
        return await _dbSet.CountAsync(r => r.Status == statusEnum);
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