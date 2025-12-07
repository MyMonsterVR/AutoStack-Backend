using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class RecoveryCodeRepository : IRecoveryCodeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RecoveryCodeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RecoveryCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecoveryCodes.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(RecoveryCode aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.RecoveryCodes.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(RecoveryCode aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.RecoveryCodes.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RecoveryCode aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.RecoveryCodes.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecoveryCodes.AnyAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets all unused recovery codes for a user
    /// </summary>
    public async Task<List<RecoveryCode>> GetUnusedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecoveryCodes
            .Where(r => r.UserId == userId && !r.IsUsed)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes all recovery codes for a user
    /// </summary>
    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var codes = await _dbContext.RecoveryCodes
            .Where(r => r.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.RecoveryCodes.RemoveRange(codes);
    }
}
