using FluentValidation;
using SDIA.SharedKernel.Enums;

namespace SDIA.Application.Registrations.Management.Upsert;

public class UpsertRegistrationCommandValidator : AbstractValidator<UpsertRegistrationCommand>
{
    public UpsertRegistrationCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters");
            
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");
            
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone format");
            
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birth date is required")
            .Must(birthDate => birthDate <= DateTime.Today)
            .WithMessage("Birth date cannot be in the future")
            .Must(birthDate => birthDate >= DateTime.Today.AddYears(-120))
            .WithMessage("Birth date is not valid");
            
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid registration status");
            
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");
            
        RuleFor(x => x.FormTemplateId)
            .NotEmpty()
            .WithMessage("Form template ID is required");
            
        // Rejection reason is required when status is Rejected
        When(x => x.Status == RegistrationStatus.Rejected, () => {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required when status is Rejected")
                .MaximumLength(1000)
                .WithMessage("Rejection reason must not exceed 1000 characters");
        });
        
        // Registration number validation for updates
        When(x => x.IsUpdate, () => {
            RuleFor(x => x.RegistrationNumber)
                .NotEmpty()
                .WithMessage("Registration number is required for updates")
                .MaximumLength(50)
                .WithMessage("Registration number must not exceed 50 characters");
        });
    }
}