using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDIA.Core.Registrations;

namespace SDIA.Infrastructure.Data.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(r => r.RegistrationNumber)
            .IsUnique();
            
        builder.Property(r => r.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(r => r.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(r => r.Phone)
            .HasMaxLength(20);
            
        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(r => r.FormData)
            .HasColumnType("nvarchar(max)");
            
        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);
            
        // Relations
        builder.HasOne(r => r.Organization)
            .WithMany(o => o.Registrations)
            .HasForeignKey(r => r.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.FormTemplate)
            .WithMany(ft => ft.Registrations)
            .HasForeignKey(r => r.FormTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.AssignedToUser)
            .WithMany(u => u.ManagedRegistrations)
            .HasForeignKey(r => r.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasMany(r => r.Documents)
            .WithOne(d => d.Registration)
            .HasForeignKey(d => d.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(r => r.History)
            .WithOne(h => h.Registration)
            .HasForeignKey(h => h.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(r => r.Comments)
            .WithOne(c => c.Registration)
            .HasForeignKey(c => c.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}