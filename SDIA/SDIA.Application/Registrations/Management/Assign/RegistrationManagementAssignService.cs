using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.Core.Users;

namespace SDIA.Application.Registrations.Management.Assign;

public class RegistrationManagementAssignService
{
    private readonly RegistrationManagementAssignValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;

    public RegistrationManagementAssignService(
        RegistrationManagementAssignValidator validator,
        IRegistrationRepository registrationRepository,
        IUserRepository userRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<RegistrationManagementAssignResult>> ExecuteAsync(
        RegistrationManagementAssignModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationManagementAssignResult>.Invalid(validationResult.ValidationErrors);
        }

        // Get the registration
        var registration = await _registrationRepository.GetByIdAsync(model.RegistrationId, cancellationToken);
        if (registration == null)
        {
            return Result<RegistrationManagementAssignResult>.NotFound("Registration not found");
        }

        User? assignedUser = null;
        string? assignedUserName = null;

        // Get user information if assigning
        if (model.AssignedToUserId.HasValue)
        {
            assignedUser = await _userRepository.GetByIdAsync(model.AssignedToUserId.Value, cancellationToken);
            if (assignedUser != null)
            {
                assignedUserName = $"{assignedUser.FirstName} {assignedUser.LastName}";
            }
        }

        // Update the assignment
        registration.AssignedToUserId = model.AssignedToUserId;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        // TODO: Add to registration history
        // TODO: Send notification to assigned user

        var result = new RegistrationManagementAssignResult
        {
            RegistrationId = registration.Id,
            AssignedToUserId = model.AssignedToUserId,
            AssignedToUserName = assignedUserName,
            AssignedAt = DateTime.UtcNow,
            Success = true,
            Message = model.AssignedToUserId.HasValue
                ? $"Registration assigned to {assignedUserName}"
                : "Registration unassigned"
        };

        return Result<RegistrationManagementAssignResult>.Success(result);
    }
}