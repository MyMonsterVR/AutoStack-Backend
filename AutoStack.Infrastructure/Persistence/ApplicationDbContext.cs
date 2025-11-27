using AutoStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Stack> Stacks => Set<Stack>();
    public DbSet<StackInfo> StackInfos => Set<StackInfo>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<CliVersion> CliVersions => Set<CliVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}