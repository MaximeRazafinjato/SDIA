using SDIA.Core.Users;
using SDIA.Core.Organizations;
using Microsoft.EntityFrameworkCore;

namespace SDIA.API.Data;

public static class SimpleDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SimpleSDIADbContext>();
        
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
        
        // Save all changes
        await context.SaveChangesAsync();
        
        Console.WriteLine("Database initialized with seed data:");
        Console.WriteLine("- Organization: SDIA Demo Organization");
        Console.WriteLine("- Admin user: admin@sdia.com / Admin123!");
        Console.WriteLine("- Manager user: manager@sdia.com / Manager123!");
        Console.WriteLine("- Regular user: user@sdia.com / User123!");
    }
}