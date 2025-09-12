using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using FluentValidation;

namespace SDIA.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);
        
        // Register application services - MediatR automatically registers all handlers
        
        return services;
    }
}