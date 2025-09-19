using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SDIA.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register all Services
        RegisterServicesFromAssembly(services, assembly);

        return services;
    }

    private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly)
    {
        // Register all Services
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            services.AddScoped(serviceType);
        }

        // Register all Validators
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Validator"))
            .ToList();

        foreach (var validatorType in validatorTypes)
        {
            services.AddScoped(validatorType);
        }
    }
}