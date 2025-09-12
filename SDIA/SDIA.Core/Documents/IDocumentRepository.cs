using SDIA.SharedKernel.Interfaces;

namespace SDIA.Core.Documents;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByRegistrationIdAsync(Guid registrationId);
    Task<IEnumerable<Document>> GetByTypeAsync(string type);
    Task<Document?> GetByFileNameAsync(string fileName);
    Task<IEnumerable<Document>> GetByUploadDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Document>> GetByFileSizeRangeAsync(long minSize, long maxSize);
    Task<long> GetTotalFileSizeAsync();
    Task<long> GetTotalFileSizeByRegistrationAsync(Guid registrationId);
    Task<IEnumerable<Document>> GetPendingValidationDocumentsAsync();
    Task<IEnumerable<Document>> GetValidatedDocumentsAsync();
    Task<IEnumerable<Document>> GetRejectedDocumentsAsync();
    Task<bool> ExistsByFileNameAsync(string fileName);
}