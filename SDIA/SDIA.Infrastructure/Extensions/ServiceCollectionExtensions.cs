using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDIA.Infrastructure.Services;
using SDIA.Infrastructure.Repositories;
using SDIA.Core.Users;
using SDIA.Core.Organizations;
using SDIA.Core.Registrations;
using SDIA.Core.FormTemplates;
using SDIA.Core.Documents;

namespace SDIA.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IRegistrationCommentRepository, RegistrationCommentRepository>();
        services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        
        // Register services (temporarily commented due to missing interfaces)
        // services.AddScoped<IEmailService, EmailService>();
        // services.AddScoped<ISmsService, SmsService>();
        // services.AddScoped<IFileStorageService, FileStorageService>();

        // Configure external services
        // services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
        // services.Configure<TwilioSettings>(configuration.GetSection("Twilio"));
        
        return services;
    }
}