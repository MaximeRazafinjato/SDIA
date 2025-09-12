using Ardalis.Result;
using MediatR;

namespace SDIA.Application.Auth.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}