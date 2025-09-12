using System.Linq.Expressions;

namespace SDIA.SharedKernel.Interfaces;

public interface IRepository<T> where T : class, IEntity
{
    // Basic CRUD operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
    // Query operations
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IQueryable<T>> GetQueryableAsync(CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    // Include operations for related data
    Task<IEnumerable<T>> GetWithIncludeAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetWithIncludeAsync(CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdWithIncludeAsync(Guid id, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includes);
}