using SDIA.Application.Common.Models;

namespace SDIA.Application.Registrations.Management.GetById;

public class GetRegistrationByIdQuery : BaseQuery<RegistrationDto>
{
    public Guid RegistrationId { get; set; }

    public GetRegistrationByIdQuery(Guid registrationId)
    {
        RegistrationId = registrationId;
    }
}