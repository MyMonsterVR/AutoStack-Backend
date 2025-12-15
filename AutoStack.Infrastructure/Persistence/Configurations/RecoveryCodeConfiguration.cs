using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the RecoveryCode entity
/// </summary>
public class RecoveryCodeConfiguration : IEntityTypeConfiguration<RecoveryCode>
{
    public void Configure(EntityTypeBuilder<RecoveryCode> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(r => r.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.UsedAt);

        builder.HasIndex(r => r.UserId);

        builder.HasIndex(r => new { r.UserId, r.IsUsed })
            .HasFilter("\"IsUsed\" = false");
    }
}