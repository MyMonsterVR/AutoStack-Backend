using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AuditLog entity
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);

        builder.Property(x => x.Exception).HasColumnType("text");
        builder.Property(x => x.Username).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.HttpMethod).HasMaxLength(10);
        builder.Property(x => x.Endpoint).HasMaxLength(500);
        builder.Property(x => x.SanitizedRequestBody).HasColumnType("text");

        // PostgreSQL-specific JSONB column for additional data
        builder.Property(x => x.AdditionalData).HasColumnType("jsonb");

        builder.HasIndex(x => x.CreatedAt).IsDescending();
        builder.HasIndex(x => x.UserId).HasFilter("\"UserId\" IS NOT NULL");
        builder.HasIndex(x => new { x.Level, x.Category });
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasFilter("\"UserId\" IS NOT NULL");
    }
}
