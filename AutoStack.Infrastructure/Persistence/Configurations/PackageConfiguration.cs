using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Link)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Unique constraint on Link - each package URL should be unique
        builder.HasIndex(x => x.Link)
            .IsUnique();

        builder.HasIndex(x => x.IsVerified);
        builder.HasIndex(x => x.Name);

        // Relationship with StackInfos
        builder.HasMany(x => x.StackInfos)
            .WithOne(x => x.Package)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete package if used in stacks

        // Configure backing field
        builder.Metadata
            .FindNavigation(nameof(Package.StackInfos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
