using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using SDIA.Core.Services;
using System.Security.Cryptography;

namespace SDIA.Application.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // For now, just log the request since we don't have DB access in Application layer
            _logger.LogInformation("Forgot password requested for email: {Email}", request.Email);
            
            // Always return success to prevent email enumeration
            return Result.Success();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for {Email}", request.Email);
            return Result.Error("Une erreur est survenue. Veuillez réessayer plus tard.");
        }
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}