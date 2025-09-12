using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SDIA.Application.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // For now, just log the request since we don't have DB access in Application layer
            _logger.LogInformation("Password reset requested with token: {Token}", request.Token);
            
            // This would be handled by the API controller for now
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return Result.Error("Une erreur est survenue lors de la réinitialisation du mot de passe");
        }
    }
}