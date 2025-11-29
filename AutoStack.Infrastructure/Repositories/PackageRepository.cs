using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class PackageRepository : IPackageRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PackageRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages.FindAsync([id], cancellationToken);
    }

    public async Task<Package?> GetByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Link == link, cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetVerifiedPackagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages
            .AsNoTracking()
            .Where(p => p.IsVerified)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages
            .AsNoTracking()
            .AnyAsync(p => p.Link == link, cancellationToken);
    }

    public async Task AddAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.Packages.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.Packages.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Package aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Packages.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Packages
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
}
