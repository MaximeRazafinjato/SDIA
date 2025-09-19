using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;

namespace SDIA.Application.Auth.ResetPassword;

public class AuthResetPasswordValidator : IValidator
{
    public Task<Result> ValidateAsync(AuthResetPasswordModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = new List<ValidationError>();

        // Token validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Token, nameof(model.Token), "Reset token is required");

        // NewPassword validation
        if (string.IsNullOrWhiteSpace(model.NewPassword))
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.NewPassword), ErrorMessage = "New password is required" });
        }
        else
        {
            if (model.NewPassword.Length < 6)
                validationErrors.Add(new ValidationError { Identifier = nameof(model.NewPassword), ErrorMessage = "Password must be at least 6 characters long" });

            validationErrors.AddErrorIfNotMatch(
                model.NewPassword,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
                nameof(model.NewPassword),
                "Password must contain at least one lowercase letter, one uppercase letter, and one digit");
        }

        // ConfirmPassword validation
        validationErrors.AddErrorIfNullOrWhiteSpace(model.ConfirmPassword, nameof(model.ConfirmPassword), "Password confirmation is required");

        if (!string.IsNullOrWhiteSpace(model.ConfirmPassword) && model.ConfirmPassword != model.NewPassword)
        {
            validationErrors.Add(new ValidationError { Identifier = nameof(model.ConfirmPassword), ErrorMessage = "Password confirmation does not match" });
        }

        return Task.FromResult(validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success());
    }
}