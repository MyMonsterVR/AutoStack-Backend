using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.RefreshTokens.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .AnyAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
    }
}