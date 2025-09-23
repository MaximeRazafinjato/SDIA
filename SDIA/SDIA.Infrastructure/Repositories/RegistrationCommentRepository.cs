using SDIA.Core.Registrations;
using SDIA.Infrastructure.Data;

namespace SDIA.Infrastructure.Repositories;

public class RegistrationCommentRepository : BaseRepository<RegistrationComment>, IRegistrationCommentRepository
{
    public RegistrationCommentRepository(SDIADbContext context) : base(context)
    {
    }
}