using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for AuditLog entity operations
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
    /// <summary>
    /// Gets the most recent audit logs
    /// </summary>
    /// <param name="count">The number of logs to retrieve</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A list of recent audit logs ordered by CreatedAt descending</returns>
    Task<List<AuditLog>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A list of audit logs for the user ordered by CreatedAt descending</returns>
    Task<List<AuditLog>> GetLogsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
