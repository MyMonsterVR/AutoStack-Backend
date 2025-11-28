using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class PackageRepository(ApplicationDbContext dbContext) : IPackageRepository
{
    public async Task<Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages.FindAsync([id], cancellationToken);
    }

    public async Task<Package?> GetByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Link == link, cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetVerifiedPackagesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages
            .AsNoTracking()
            .Where(p => p.IsVerified)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages
            .AsNoTracking()
            .AnyAsync(p => p.Link == link, cancellationToken);
    }

    public async Task AddAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        await dbContext.Packages.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        dbContext.Packages.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.Packages.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Packages
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}
