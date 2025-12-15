using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class StackVoteRepository : IStackVoteRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StackVoteRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StackVote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StackVotes.FindAsync([id], cancellationToken);
    }

    public async Task<StackVote?> GetByUserAndStackAsync(Guid userId, Guid stackId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StackVotes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.StackId == stackId, cancellationToken);
    }

    public async Task<IEnumerable<StackVote>> GetByStackIdAsync(Guid stackId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StackVotes
            .AsNoTracking()
            .Where(v => v.StackId == stackId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserVotedAsync(Guid userId, Guid stackId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StackVotes
            .AsNoTracking()
            .AnyAsync(v => v.UserId == userId && v.StackId == stackId, cancellationToken);
    }

    public async Task AddAsync(StackVote aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.StackVotes.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(StackVote aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.StackVotes.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(StackVote aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.StackVotes.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StackVotes
            .AsNoTracking()
            .AnyAsync(v => v.Id == id, cancellationToken);
    }
}
