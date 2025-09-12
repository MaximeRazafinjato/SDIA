using Ardalis.Result;
using MediatR;

namespace SDIA.Application.Auth.ValidateResetToken;

public class ValidateResetTokenQuery : IRequest<Result<ValidateResetTokenDto>>
{
    public string Token { get; set; } = string.Empty;
}