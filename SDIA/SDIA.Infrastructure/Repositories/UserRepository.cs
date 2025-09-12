using Microsoft.EntityFrameworkCore;
using SDIA.Core.Users;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(SDIADbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await _dbSet
            .Where(u => u.OrganizationId == organizationId)
            .Include(u => u.Organization)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .Include(u => u.Organization)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailConfirmedAsync(string email)
    {
        var user = await GetByEmailAsync(email);
        return user?.EmailConfirmed ?? false;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .Include(u => u.Organization)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetInactiveUsersAsync()
    {
        return await _dbSet
            .Where(u => !u.IsActive)
            .Include(u => u.Organization)
            .ToListAsync();
    }

    public async Task ConfirmEmailAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ConfirmPhoneAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.PhoneConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastLoginAsync(Guid userId, DateTime loginTime)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = loginTime;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry)
    {
        var user = await GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expiry;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}