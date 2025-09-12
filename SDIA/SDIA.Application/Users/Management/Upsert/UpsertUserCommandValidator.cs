using FluentValidation;

namespace SDIA.Application.Users.Management.Upsert;

public class UpsertUserCommandValidator : AbstractValidator<UpsertUserCommand>
{
    public UpsertUserCommandValidator()
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
            
        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .Must(role => new[] { "Admin", "Manager", "User" }.Contains(role))
            .WithMessage("Role must be Admin, Manager, or User");
            
        // Password is required for new users
        When(x => !x.IsUpdate, () => {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required for new users")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");
        });
        
        // Optional password validation for updates
        When(x => x.IsUpdate && !string.IsNullOrEmpty(x.Password), () => {
            RuleFor(x => x.Password)
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");
        });
    }
}