using Microsoft.EntityFrameworkCore;
using SDIA.Core.Documents;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
{
    public DocumentRepository(SDIADbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetByRegistrationIdAsync(Guid registrationId)
    {
        return await _dbSet
            .Where(d => d.RegistrationId == registrationId)
            .Include(d => d.Registration)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByTypeAsync(string type)
    {
        // Type property doesn't exist, using DocumentType instead
        return await _dbSet
            .Where(d => d.DocumentType == type)
            .Include(d => d.Registration)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Document?> GetByFileNameAsync(string fileName)
    {
        return await _dbSet
            .Where(d => d.FileName == fileName)
            .Include(d => d.Registration)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Document>> GetByUploadDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
            .Include(d => d.Registration)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByFileSizeRangeAsync(long minSize, long maxSize)
    {
        return await _dbSet
            .Where(d => d.FileSize >= minSize && d.FileSize <= maxSize)
            .Include(d => d.Registration)
            .OrderByDescending(d => d.FileSize)
            .ToListAsync();
    }

    public async Task<long> GetTotalFileSizeAsync()
    {
        return await _dbSet.SumAsync(d => d.FileSize);
    }

    public async Task<long> GetTotalFileSizeByRegistrationAsync(Guid registrationId)
    {
        return await _dbSet
            .Where(d => d.RegistrationId == registrationId)
            .SumAsync(d => d.FileSize);
    }

    public async Task<IEnumerable<Document>> GetPendingValidationDocumentsAsync()
    {
        return await _dbSet
            // ValidationStatus property doesn't exist yet
            .Where(d => false) // Placeholder - will return empty
            .Include(d => d.Registration)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetValidatedDocumentsAsync()
    {
        return await _dbSet
            // ValidationStatus property doesn't exist yet
            .Where(d => false) // Placeholder - will return empty
            .Include(d => d.Registration)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetRejectedDocumentsAsync()
    {
        return await _dbSet
            // ValidationStatus property doesn't exist yet
            .Where(d => false) // Placeholder - will return empty
            .Include(d => d.Registration)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByFileNameAsync(string fileName)
    {
        return await _dbSet.AnyAsync(d => d.FileName == fileName);
    }
}