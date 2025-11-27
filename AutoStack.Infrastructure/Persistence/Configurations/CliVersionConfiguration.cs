using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

public class CliVersionConfiguration : IEntityTypeConfiguration<CliVersion>
{
    public void Configure(EntityTypeBuilder<CliVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version).IsRequired().HasMaxLength(8);
    }
}
