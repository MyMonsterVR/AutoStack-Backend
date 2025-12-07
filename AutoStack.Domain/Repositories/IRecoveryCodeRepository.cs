using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface IRecoveryCodeRepository : IRepository<RecoveryCode, Guid>
{
    /// <summary>
    /// Gets all unused recovery codes for a user
    /// </summary>
    Task<List<RecoveryCode>> GetUnusedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all recovery codes for a user
    /// </summary>
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}