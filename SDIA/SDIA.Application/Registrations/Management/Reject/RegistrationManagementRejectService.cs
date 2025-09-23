using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Reject;

public class RegistrationManagementRejectService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IRegistrationCommentRepository _commentRepository;

    public RegistrationManagementRejectService(
        IRegistrationRepository registrationRepository,
        IRegistrationCommentRepository commentRepository)
    {
        _registrationRepository = registrationRepository;
        _commentRepository = commentRepository;
    }

    public async Task<Result> ExecuteAsync(
        RegistrationManagementRejectModel model,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(model.Reason))
        {
            return Result.Invalid(new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = "Reason",
                    ErrorMessage = "La raison du rejet est obligatoire"
                }
            });
        }

        var registration = await _registrationRepository.GetAll(cancellationToken)
            .FirstOrDefaultAsync(r => r.Id == model.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.NotFound("Inscription non trouvée");
        }

        registration.Status = RegistrationStatus.Rejected;
        registration.RejectedAt = DateTime.UtcNow;
        registration.RejectionReason = model.Reason;
        registration.UpdatedAt = DateTime.UtcNow;

        // Add rejection reason as comment
        var comment = new RegistrationComment
        {
            Id = Guid.NewGuid(),
            Content = $"Dossier rejeté : {model.Reason}",
            RegistrationId = model.RegistrationId,
            AuthorId = userId,
            IsInternal = false, // Visible to applicant
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _registrationRepository.UpdateAsync(registration, cancellationToken);

        return Result.Success();
    }
}

public class RegistrationManagementRejectModel
{
    public Guid RegistrationId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comments { get; set; }
}