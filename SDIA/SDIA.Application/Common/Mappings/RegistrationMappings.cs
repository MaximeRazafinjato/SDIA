using System.Linq.Expressions;
using SDIA.Application.Registrations.Management.Grid;
using SDIA.Application.Registrations.Management.GetById;
using SDIA.Core.Registrations;

namespace SDIA.Application.Common.Mappings;

public static class RegistrationMappings
{
    public static Expression<Func<Registration, RegistrationManagementGridModel>> ToGridModel()
    {
        return reg => new RegistrationManagementGridModel
        {
            Id = reg.Id,
            RegistrationNumber = reg.RegistrationNumber,
            Status = reg.Status,
            FirstName = reg.FirstName,
            LastName = reg.LastName,
            Email = reg.Email,
            Phone = reg.Phone,
            BirthDate = reg.BirthDate,
            IsMinor = reg.IsMinor,
            SubmittedAt = reg.SubmittedAt,
            EmailVerified = reg.EmailVerified,
            PhoneVerified = reg.PhoneVerified,
            OrganizationName = reg.Organization != null ? reg.Organization.Name : null,
            FormTemplateName = reg.FormTemplate != null ? reg.FormTemplate.Name : null,
            AssignedToUserName = reg.AssignedToUser != null ?
                reg.AssignedToUser.FirstName + " " + reg.AssignedToUser.LastName : null,
            CreatedAt = reg.CreatedAt
        };
    }

    public static Expression<Func<Registration, RegistrationManagementGetByIdModel>> ToDetailModel()
    {
        return reg => new RegistrationManagementGetByIdModel
        {
            Id = reg.Id,
            RegistrationNumber = reg.RegistrationNumber,
            Status = reg.Status,
            FirstName = reg.FirstName,
            LastName = reg.LastName,
            Email = reg.Email,
            Phone = reg.Phone,
            BirthDate = reg.BirthDate,
            IsMinor = reg.IsMinor,
            FormData = reg.FormData,
            SubmittedAt = reg.SubmittedAt,
            ValidatedAt = reg.ValidatedAt,
            RejectedAt = reg.RejectedAt,
            RejectionReason = reg.RejectionReason,
            EmailVerified = reg.EmailVerified,
            PhoneVerified = reg.PhoneVerified,
            OrganizationId = reg.OrganizationId,
            OrganizationName = reg.Organization != null ? reg.Organization.Name : null,
            FormTemplateId = reg.FormTemplateId,
            FormTemplateName = reg.FormTemplate != null ? reg.FormTemplate.Name : null,
            AssignedToUserId = reg.AssignedToUserId,
            AssignedToUserName = reg.AssignedToUser != null ?
                reg.AssignedToUser.FirstName + " " + reg.AssignedToUser.LastName : null,
            CreatedAt = reg.CreatedAt,
            UpdatedAt = reg.UpdatedAt
        };
    }
}