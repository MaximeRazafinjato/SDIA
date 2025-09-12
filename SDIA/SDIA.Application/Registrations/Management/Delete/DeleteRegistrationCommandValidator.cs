using FluentValidation;

namespace SDIA.Application.Registrations.Management.Delete;

public class DeleteRegistrationCommandValidator : AbstractValidator<DeleteRegistrationCommand>
{
    public DeleteRegistrationCommandValidator()
    {
        RuleFor(x => x.RegistrationId)
            .NotEmpty()
            .WithMessage("Registration ID is required");
    }
}