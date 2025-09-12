using Ardalis.Result;
using MediatR;

namespace SDIA.Application.Common.Models;

public abstract class BaseQuery<TResponse> : IRequest<Result<TResponse>>
{
}