using Ardalis.Result;
using SDIA.Core.Users;

namespace SDIA.Application.Users.Management.Statistics;

public class UserManagementStatisticsService
{
    private readonly IUserRepository _userRepository;

    public UserManagementStatisticsService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementStatisticsResult>> ExecuteAsync(
        UserManagementStatisticsModel model,
        CancellationToken cancellationToken = default)
    {
        var allUsers = _userRepository.GetAll(cancellationToken);

        // Apply organization filter if specified
        if (model.OrganizationId.HasValue)
        {
            allUsers = allUsers.Where(u => u.OrganizationId == model.OrganizationId.Value);
        }

        // Apply date range filter if specified
        if (model.FromDate.HasValue)
        {
            allUsers = allUsers.Where(u => u.CreatedAt >= model.FromDate.Value);
        }

        if (model.ToDate.HasValue)
        {
            allUsers = allUsers.Where(u => u.CreatedAt <= model.ToDate.Value);
        }

        // Execute queries
        var users = await Task.Run(() => allUsers.ToList(), cancellationToken);

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var result = new UserManagementStatisticsResult
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            EmailConfirmedUsers = users.Count(u => u.EmailConfirmed),
            PhoneConfirmedUsers = users.Count(u => u.PhoneConfirmed),
            NewUsersToday = users.Count(u => u.CreatedAt.Date == today),
            NewUsersThisWeek = users.Count(u => u.CreatedAt.Date >= weekStart),
            NewUsersThisMonth = users.Count(u => u.CreatedAt.Date >= monthStart),
            UsersByRole = users.GroupBy(u => u.Role).ToDictionary(g => g.Key, g => g.Count()),
            UsersByOrganization = users.Where(u => u.Organization != null).GroupBy(u => u.Organization!.Name).ToDictionary(g => g.Key, g => g.Count()),
            GeneratedAt = now
        };

        // Generate recent activity (last 7 days)
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var newUsers = users.Count(u => u.CreatedAt.Date == date);
            var logins = users.Count(u => u.LastLoginAt?.Date == date);

            result.RecentActivity.Add(new UserActivityStat
            {
                Date = date,
                NewUserCount = newUsers,
                LoginCount = logins
            });
        }

        return Result<UserManagementStatisticsResult>.Success(result);
    }
}