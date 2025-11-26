using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for RefreshToken entity operations
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>
    /// Gets a refresh token by its token string value
    /// </summary>
    /// <param name="refreshToken">The refresh token string to search for</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The refresh token if found, null otherwise</returns>
    Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken);
}