using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .HasColumnName("RefreshToken")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnType("int")
            .HasComment("Epoch time that the refresh token expires at.")
            .IsRequired();
        
        builder.Property(r => r.UserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        
        builder.HasIndex(r => r.Token)
            .IsUnique();
    }
}