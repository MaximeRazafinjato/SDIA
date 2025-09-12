using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDIA.Core.Users;

namespace SDIA.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.Phone)
            .HasMaxLength(20);
            
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
            
        // Relations
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}