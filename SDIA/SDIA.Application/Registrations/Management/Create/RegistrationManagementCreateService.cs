using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.Core.Users;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Create;

public class RegistrationManagementCreateService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationManagementCreateService(
        IRegistrationRepository registrationRepository,
        IUserRepository userRepository)
    {
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RegistrationManagementCreateResult>> ExecuteAsync(
        RegistrationManagementCreateModel model,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<RegistrationManagementCreateResult>.Unauthorized();
        }

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = $"REG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Status = RegistrationStatus.Draft,
            OrganizationId = user.OrganizationId ?? Guid.Empty,
            FormTemplateId = model.FormTemplateId,
            FormData = model.FormData,
            BirthDate = model.BirthDate ?? DateTime.MinValue,
            CreatedAt = DateTime.UtcNow
        };

        await _registrationRepository.AddAsync(registration, cancellationToken);

        var result = new RegistrationManagementCreateResult
        {
            Id = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            Status = registration.Status.ToString(),
            CreatedAt = registration.CreatedAt
        };

        return Result<RegistrationManagementCreateResult>.Success(result);
    }
}

public class RegistrationManagementCreateModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid FormTemplateId { get; set; }
    public string? FormData { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class RegistrationManagementCreateResult
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}