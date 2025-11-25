using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

public class StackConfiguration : IEntityTypeConfiguration<Stack>
{
    public void Configure(EntityTypeBuilder<Stack> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Downloads)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.UserId)
            .IsRequired();

        // Relationship with User
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with StackInfo - configure backing field
        builder.HasMany(x => x.StackInfo)
            .WithOne(x => x.Stack)
            .HasForeignKey(x => x.StackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tell EF Core to use the backing field
        builder.Metadata
            .FindNavigation(nameof(Stack.StackInfo))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Name);
    }
}
