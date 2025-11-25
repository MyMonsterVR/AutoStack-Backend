using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class StackRepository(ApplicationDbContext dbContext) : IStackRepository
{
    public async Task<Stack?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Stacks.FindAsync([id], cancellationToken);
    }

    public async Task<Stack?> GetByIdWithInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Stacks
            .Include(s => s.StackInfo)
                .ThenInclude(si => si.Package)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Stack>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Stacks
            .Include(s => s.StackInfo)
                .ThenInclude(si => si.Package)
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Stack>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Stacks
            .Include(s => s.StackInfo)
                .ThenInclude(si => si.Package)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        await dbContext.Stacks.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        dbContext.Stacks.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Stack aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.Stacks.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Stacks.AnyAsync(s => s.Id == id, cancellationToken);
    }
}