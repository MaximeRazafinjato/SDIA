using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SDIA.Application.Auth.ValidateResetToken;

public class ValidateResetTokenQueryHandler : IRequestHandler<ValidateResetTokenQuery, Result<ValidateResetTokenDto>>
{
    private readonly ILogger<ValidateResetTokenQueryHandler> _logger;

    public ValidateResetTokenQueryHandler(
        ILogger<ValidateResetTokenQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<ValidateResetTokenDto>> Handle(ValidateResetTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Result<ValidateResetTokenDto>.Success(new ValidateResetTokenDto { IsValid = false });
            }

            // For now, just log the request since we don't have DB access in Application layer
            _logger.LogInformation("Token validation requested: {Token}", request.Token);
            
            // This would be handled by the API controller for now
            return Result<ValidateResetTokenDto>.Success(new ValidateResetTokenDto
            {
                IsValid = false,
                Email = string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reset token");
            return Result<ValidateResetTokenDto>.Error("Une erreur est survenue lors de la validation du token");
        }
    }
}