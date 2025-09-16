using SDIA.Core.Users;
using SDIA.Core.Organizations;
using SDIA.Core.FormTemplates;
using SDIA.Core.Registrations;
using SDIA.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace SDIA.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SDIADbContext>();
        
        // Ensure database is created - commented out since database already exists
        // await context.Database.EnsureCreatedAsync();
        
        // Check if we already have data
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }
        
        // Create default organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "SDIA Demo Organization",
            Code = "SDIA-001",
            Type = "CFA",
            Email = "contact@sdia-demo.com",
            Phone = "+33123456789",
            Address = "1 rue de la Formation",
            City = "Paris",
            PostalCode = "75001",
            Country = "France",
            Website = "https://sdia-demo.com",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Organizations.Add(organization);
        
        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "SDIA",
            Email = "admin@sdia.com",
            Phone = "+33600000000",
            Role = "Admin",
            IsActive = true,
            EmailConfirmed = true,
            PhoneConfirmed = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        // Set password to "Admin123!"
        adminUser.SetPassword("Admin123!");
        
        context.Users.Add(adminUser);
        
        // Create manager user
        var managerUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Manager",
            LastName = "Test",
            Email = "manager@sdia.com",
            Phone = "+33600000001",
            Role = "Manager",
            IsActive = true,
            EmailConfirmed = true,
            PhoneConfirmed = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        managerUser.SetPassword("Manager123!");
        context.Users.Add(managerUser);
        
        // Create regular user
        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "User",
            LastName = "Test",
            Email = "user@sdia.com",
            Phone = "+33600000002",
            Role = "User",
            IsActive = true,
            EmailConfirmed = true,
            PhoneConfirmed = false,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        regularUser.SetPassword("User123!");
        context.Users.Add(regularUser);
        
        // Create sample form template
        var formTemplate = new FormTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Formulaire d'inscription standard",
            Description = "Formulaire standard pour les nouvelles inscriptions",
            IsActive = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow,
            FormSchema = """
                {
                    "fields": [
                        {
                            "id": "firstName",
                            "type": "text",
                            "label": "Prénom",
                            "required": true,
                            "placeholder": "Votre prénom"
                        },
                        {
                            "id": "lastName",
                            "type": "text",
                            "label": "Nom",
                            "required": true,
                            "placeholder": "Votre nom"
                        },
                        {
                            "id": "email",
                            "type": "email",
                            "label": "Email",
                            "required": true,
                            "placeholder": "votre.email@example.com"
                        },
                        {
                            "id": "phone",
                            "type": "tel",
                            "label": "Téléphone",
                            "placeholder": "+33123456789"
                        },
                        {
                            "id": "birthDate",
                            "type": "date",
                            "label": "Date de naissance",
                            "required": true
                        }
                    ]
                }
                """
        };
        
        context.FormTemplates.Add(formTemplate);
        
        // Create sample registrations
        var registration1 = new Registration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "REG-2025-001",
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@example.com",
            Phone = "+33612345678",
            Status = RegistrationStatus.Validated,
            OrganizationId = organization.Id,
            FormTemplateId = formTemplate.Id,
            AssignedToUserId = adminUser.Id,
            BirthDate = new DateTime(1990, 5, 15),
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            SubmittedAt = DateTime.UtcNow.AddDays(-6),
            FormData = """{"firstName":"Jean","lastName":"Dupont","email":"jean.dupont@example.com","phone":"+33612345678","birthDate":"1990-05-15"}"""
        };
        
        var registration2 = new Registration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "REG-2025-002",
            FirstName = "Marie",
            LastName = "Martin",
            Email = "marie.martin@example.com",
            Phone = "+33687654321",
            Status = RegistrationStatus.Pending,
            OrganizationId = organization.Id,
            FormTemplateId = formTemplate.Id,
            AssignedToUserId = managerUser.Id,
            BirthDate = new DateTime(1985, 8, 22),
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            SubmittedAt = DateTime.UtcNow.AddDays(-2),
            FormData = """{"firstName":"Marie","lastName":"Martin","email":"marie.martin@example.com","phone":"+33687654321","birthDate":"1985-08-22"}"""
        };
        
        var registration3 = new Registration
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "REG-2025-003",
            FirstName = "Pierre",
            LastName = "Bernard",
            Email = "pierre.bernard@example.com",
            Phone = "+33654321098",
            Status = RegistrationStatus.Draft,
            OrganizationId = organization.Id,
            FormTemplateId = formTemplate.Id,
            BirthDate = new DateTime(2008, 3, 10),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            FormData = """{"firstName":"Pierre","lastName":"Bernard","email":"pierre.bernard@example.com","phone":"+33654321098","birthDate":"2008-03-10"}"""
        };
        
        context.Registrations.Add(registration1);
        context.Registrations.Add(registration2);
        context.Registrations.Add(registration3);
        
        // Save all changes
        await context.SaveChangesAsync();
        
        Console.WriteLine("Database initialized with seed data:");
        Console.WriteLine("- Organization: SDIA Demo Organization");
        Console.WriteLine("- Admin user: admin@sdia.com / Admin123!");
        Console.WriteLine("- Manager user: manager@sdia.com / Manager123!");
        Console.WriteLine("- Regular user: user@sdia.com / User123!");
        Console.WriteLine("- 1 Form template created");
        Console.WriteLine("- 3 Sample registrations created (1 validated, 1 pending, 1 draft)");
    }
}