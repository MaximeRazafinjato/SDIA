using System.Linq.Expressions;
using SDIA.Application.Users.Management.Grid;
using SDIA.Application.Users.Management.GetById;
using SDIA.Core.Users;

namespace SDIA.Application.Common.Mappings;

public static class UserMappings
{
    public static Expression<Func<User, UserManagementGridModel>> ToGridModel()
    {
        return user => new UserManagementGridModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed,
            LastLoginAt = user.LastLoginAt,
            OrganizationName = user.Organization != null ? user.Organization.Name : null,
            CreatedAt = user.CreatedAt
        };
    }

    public static Expression<Func<User, UserManagementGetByIdModel>> ToDetailModel()
    {
        return user => new UserManagementGetByIdModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneConfirmed,
            LastLoginAt = user.LastLoginAt,
            OrganizationId = user.OrganizationId,
            OrganizationName = user.Organization != null ? user.Organization.Name : null,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}