using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Application.Common.Mappings;
using SDIA.Core.Registrations;

namespace SDIA.Application.Registrations.Management.GetById;

public class RegistrationManagementGetByIdService
{
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationManagementGetByIdService(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationManagementGetByIdModel>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _registrationRepository.GetAll(cancellationToken)
            .Where(r => r.Id == id)
            .Select(RegistrationMappings.ToDetailModel())
            .FirstOrDefaultAsync(cancellationToken);

        if (model == null)
        {
            return Result<RegistrationManagementGetByIdModel>.NotFound("Registration not found");
        }

        return Result<RegistrationManagementGetByIdModel>.Success(model);
    }
}