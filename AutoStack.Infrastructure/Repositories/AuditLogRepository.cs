using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

/// <summary>
/// Repository for AuditLog entity operations
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(AuditLog aggregate, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(AuditLog aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        _dbContext.AuditLogs.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AuditLog aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.AuditLogs.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .AnyAsync(log => log.Id == id, cancellationToken);
    }

    public async Task<List<AuditLog>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .OrderByDescending(log => log.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetLogsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
