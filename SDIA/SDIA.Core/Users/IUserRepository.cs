using SDIA.SharedKernel.Interfaces;

namespace SDIA.Core.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<bool> ExistsAsync(string email);
    Task<bool> IsEmailConfirmedAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetInactiveUsersAsync();
    Task ConfirmEmailAsync(Guid userId);
    Task ConfirmPhoneAsync(Guid userId);
    Task UpdateLastLoginAsync(Guid userId, DateTime loginTime);
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry);
}