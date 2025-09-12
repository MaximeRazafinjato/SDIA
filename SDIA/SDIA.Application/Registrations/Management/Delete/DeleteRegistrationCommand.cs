using SDIA.Application.Common.Models;

namespace SDIA.Application.Registrations.Management.Delete;

public class DeleteRegistrationCommand : BaseCommand
{
    public Guid RegistrationId { get; set; }

    public DeleteRegistrationCommand(Guid registrationId)
    {
        RegistrationId = registrationId;
    }
}