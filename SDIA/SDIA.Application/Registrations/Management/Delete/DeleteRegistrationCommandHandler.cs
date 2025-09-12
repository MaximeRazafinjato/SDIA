using Ardalis.Result;
using MediatR;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.Delete;

public class DeleteRegistrationCommandHandler : IRequestHandler<DeleteRegistrationCommand, Result>
{
    private readonly IRegistrationRepository _registrationRepository;

    public DeleteRegistrationCommandHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result> Handle(DeleteRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _registrationRepository.GetByIdAsync(request.RegistrationId, cancellationToken);
        
        if (registration == null)
        {
            return Result.NotFound("Registration not found");
        }

        // Soft delete
        registration.IsDeleted = true;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result.Success();
    }
}