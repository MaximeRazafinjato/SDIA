using Microsoft.EntityFrameworkCore;
using SDIA.Core.Users;
using SDIA.Core.Organizations;

namespace SDIA.API.Data;

public class SimpleSDIADbContext : DbContext
{
    public SimpleSDIADbContext(DbContextOptions<SimpleSDIADbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Global query filters for soft delete
        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is SharedKernel.Models.BaseEntity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}