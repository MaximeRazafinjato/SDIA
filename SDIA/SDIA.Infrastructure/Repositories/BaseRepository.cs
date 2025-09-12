using Microsoft.EntityFrameworkCore;
using SDIA.Infrastructure.Data;
using SDIA.SharedKernel.Interfaces;
using System.Linq.Expressions;

namespace SDIA.Infrastructure.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly SDIADbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(SDIADbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(int page, int pageSize)
    {
        return await _dbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(entity);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        await UpdateRangeAsync(entities);
    }

    public virtual async Task<IEnumerable<T>> GetWithIncludeAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync();
    }

    public virtual async Task<T?> GetByIdWithIncludeAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }
}