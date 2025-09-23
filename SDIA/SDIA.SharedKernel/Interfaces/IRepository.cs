using System.Linq.Expressions;

namespace SDIA.SharedKernel.Interfaces;

public interface IRepository<T> where T : class, IEntity
{
    IQueryable<T> GetAll(CancellationToken cancellationToken = default);
    // Basic CRUD operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    // Bulk operations
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    // Include operations for related data
    Task<T?> GetByIdWithIncludeAsync(Guid id, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);
}