using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Token);

        builder.Property(r => r.Token)
            .HasColumnName("RefreshToken")
            .HasColumnType("nvarchar")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .IsRequired();
        
        builder.Property(r => r.UserId)
            .IsRequired();
    }
}