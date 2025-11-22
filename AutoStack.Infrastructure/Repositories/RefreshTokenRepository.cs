using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshTokens.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        dbContext.RefreshTokens.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.RefreshTokens.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens.FindAsync([id], cancellationToken) != null;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
    }
}