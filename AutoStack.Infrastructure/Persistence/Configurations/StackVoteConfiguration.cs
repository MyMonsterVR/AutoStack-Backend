using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the StackVote entity
/// </summary>
public class StackVoteConfiguration : IEntityTypeConfiguration<StackVote>
{
    public void Configure(EntityTypeBuilder<StackVote> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StackId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IsUpvote)
            .IsRequired();

        // Relationship with Stack
        builder.HasOne(x => x.Stack)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.StackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index to ensure one vote per user per stack
        builder.HasIndex(x => new { x.UserId, x.StackId })
            .IsUnique();

        builder.HasIndex(x => x.StackId);
        builder.HasIndex(x => x.UserId);
    }
}
