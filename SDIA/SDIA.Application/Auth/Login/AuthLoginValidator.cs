using Ardalis.Result;
using SDIA.Application._Abstractions;
using SDIA.Application._Extensions;

namespace SDIA.Application.Auth.Login;

public class AuthLoginValidator : IValidator
{
    public async Task<Result> ValidateAsync(object model, CancellationToken cancellationToken = default, Guid? id = null)
    {
        var authModel = model as AuthLoginModel;
        if (authModel == null)
        {
            return Result.Error("Invalid model type");
        }

        var validationErrors = new List<ValidationError>();

        validationErrors.AddErrorIfNullOrWhiteSpace(authModel.Email, nameof(authModel.Email), "Email is required");
        validationErrors.AddErrorIfNotEmail(authModel.Email, nameof(authModel.Email), "Invalid email format");
        validationErrors.AddErrorIfNullOrWhiteSpace(authModel.Password, nameof(authModel.Password), "Password is required");

        return await Task.FromResult(validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success());
    }
}