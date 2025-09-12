using FluentValidation;

namespace SDIA.Application.FormTemplates.Management.Delete;

public class DeleteFormTemplateCommandValidator : AbstractValidator<DeleteFormTemplateCommand>
{
    public DeleteFormTemplateCommandValidator()
    {
        RuleFor(x => x.FormTemplateId)
            .NotEmpty()
            .WithMessage("Form template ID is required");
    }
}