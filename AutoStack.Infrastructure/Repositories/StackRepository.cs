using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class StackRepository : IStackRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StackRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Stack?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stacks.FindAsync([id], cancellationToken);
    }

    public async Task<Stack?> GetByIdWithInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stacks
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Packages)
                .ThenInclude(si => si.Package)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Stack>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stacks
            .AsNoTracking()
            .Include(s => s.Packages)
                .ThenInclude(si => si.Package)
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stack>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stacks
            .AsNoTracking()
            .Include(s => s.Packages)
                .ThenInclude(si => si.Package)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.Stacks.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.Stacks.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Stacks.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stacks
            .AsNoTracking()
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Stack> Stacks, int TotalCount)> GetStacksPagedAsync(
        int pageNumber,
        int pageSize,
        string? stackType,
        string sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Stacks
            .Include(s => s.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(stackType))
        {
            query = query.Where(s => s.Type == stackType);
        }

        query = sortBy.ToLowerInvariant() switch
        {
            "popularity" => sortDescending
                ? query.OrderByDescending(s => s.Downloads)
                : query.OrderBy(s => s.Downloads),
            "createddate" => sortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.Downloads) // Default sort
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var stacks = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (stacks, totalCount);
    }
}