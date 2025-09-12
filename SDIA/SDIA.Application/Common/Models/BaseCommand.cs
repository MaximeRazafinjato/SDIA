using Ardalis.Result;
using MediatR;

namespace SDIA.Application.Common.Models;

public abstract class BaseCommand<TResponse> : IRequest<Result<TResponse>>
{
}

public abstract class BaseCommand : IRequest<Result>
{
}