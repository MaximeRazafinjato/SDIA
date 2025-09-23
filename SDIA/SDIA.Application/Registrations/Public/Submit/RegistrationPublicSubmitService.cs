using Ardalis.Result;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using System.Security.Cryptography;

namespace SDIA.Application.Registrations.Public.Submit;

public class RegistrationPublicSubmitService
{
    private readonly RegistrationPublicSubmitValidator _validator;
    private readonly IRegistrationRepository _registrationRepository;

    public RegistrationPublicSubmitService(
        RegistrationPublicSubmitValidator validator,
        IRegistrationRepository registrationRepository)
    {
        _validator = validator;
        _registrationRepository = registrationRepository;
    }

    public async Task<Result<RegistrationPublicSubmitResult>> ExecuteAsync(
        RegistrationPublicSubmitModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate the model
        var validationResult = await _validator.ValidateAsync(model, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return Result<RegistrationPublicSubmitResult>.Invalid(validationResult.ValidationErrors);
        }

        // Generate registration number
        var registrationNumber = await GenerateRegistrationNumber();

        // Generate access token
        var accessToken = GenerateAccessToken();
        var accessTokenExpiry = DateTime.UtcNow.AddDays(30); // 30 days validity

        // Create registration
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = registrationNumber,
            Status = RegistrationStatus.Draft,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            BirthDate = model.BirthDate,
            FormTemplateId = model.FormTemplateId,
            OrganizationId = model.OrganizationId,
            FormData = model.FormData,
            AccessToken = accessToken,
            AccessTokenExpiry = accessTokenExpiry,
            EmailVerified = false,
            PhoneVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _registrationRepository.AddAsync(registration, cancellationToken);

        // TODO: Send initial email with access link

        var result = new RegistrationPublicSubmitResult
        {
            RegistrationId = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            AccessToken = accessToken,
            AccessTokenExpiry = accessTokenExpiry,
            Status = registration.Status.ToString(),
            Success = true,
            Message = "Registration created successfully",
            AccessUrl = $"/public/registration-access/{accessToken}/request-code"
        };

        return Result<RegistrationPublicSubmitResult>.Success(result);
    }

    private async Task<string> GenerateRegistrationNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"REG{year}";

        // Get next sequence number - simplified approach
        var random = new Random();
        var sequence = random.Next(1000, 9999);

        return $"{prefix}{sequence:D4}";
    }

    private static string GenerateAccessToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}