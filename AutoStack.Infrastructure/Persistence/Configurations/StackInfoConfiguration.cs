using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the StackInfo entity
/// </summary>
public class StackInfoConfiguration : IEntityTypeConfiguration<StackInfo>
{
    public void Configure(EntityTypeBuilder<StackInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StackId)
            .IsRequired();

        builder.Property(x => x.PackageId)
            .IsRequired();

        builder.HasIndex(x => x.StackId);
        builder.HasIndex(x => x.PackageId);

        // Prevent duplicate packages within the same stack
        builder.HasIndex(x => new { x.StackId, x.PackageId })
            .IsUnique();
    }
}
