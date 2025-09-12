using FluentValidation;
using System.Text.RegularExpressions;

namespace SDIA.Application.Auth.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Le token est requis");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Le nouveau mot de passe est requis")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule")
            .Matches("[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre")
            .Matches("[^a-zA-Z0-9]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmation du mot de passe est requise")
            .Equal(x => x.NewPassword).WithMessage("Les mots de passe ne correspondent pas");
    }
}