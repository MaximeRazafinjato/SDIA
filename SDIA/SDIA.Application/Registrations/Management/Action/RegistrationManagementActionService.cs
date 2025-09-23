using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Action;

public class RegistrationManagementActionService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementActionService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> ExecuteAsync(
        RegistrationManagementActionModel model,
        CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r => r.Id == model.Id, cancellationToken);

        if (registration == null)
        {
            return Result.NotFound("Inscription non trouvée");
        }

        var action = model.ActionType.ToLower();
        switch (action)
        {
            case "validate":
                registration.Status = RegistrationStatus.Validated;
                break;
            case "reject":
                registration.Status = RegistrationStatus.Rejected;
                break;
            case "submit":
                registration.Status = RegistrationStatus.Pending;
                registration.SubmittedAt = DateTime.UtcNow;
                break;
            default:
                return Result.Invalid(new List<ValidationError>
                {
                    new ValidationError
                    {
                        Identifier = "ActionType",
                        ErrorMessage = "Action non reconnue"
                    }
                });
        }

        registration.UpdatedAt = DateTime.UtcNow;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result.Success();
    }
}

public class RegistrationManagementActionModel
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
}