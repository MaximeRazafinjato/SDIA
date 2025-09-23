namespace SDIA.Application.Users.Management.Statistics;

public class UserManagementStatisticsModel
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? OrganizationId { get; set; }
}

public class UserManagementStatisticsResult
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int EmailConfirmedUsers { get; set; }
    public int PhoneConfirmedUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public Dictionary<string, int> UsersByOrganization { get; set; } = new();
    public List<UserActivityStat> RecentActivity { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class UserActivityStat
{
    public DateTime Date { get; set; }
    public int LoginCount { get; set; }
    public int NewUserCount { get; set; }
}