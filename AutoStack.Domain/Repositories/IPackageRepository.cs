using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for Package entity operations
/// </summary>
public interface IPackageRepository : IRepository<Package, Guid>
{
    /// <summary>
    /// Gets a package by its URL link
    /// </summary>
    /// <param name="link">The URL link of the package</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The package if found, null otherwise</returns>
    Task<Package?> GetByLinkAsync(string link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all packages that have been verified by an administrator
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of verified packages</returns>
    Task<IEnumerable<Package>> GetVerifiedPackagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all packages in the system
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of all packages</returns>
    Task<IEnumerable<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a package with the specified link already exists
    /// </summary>
    /// <param name="link">The URL link to check</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if a package with the link exists, false otherwise</returns>
    Task<bool> ExistsByLinkAsync(string link, CancellationToken cancellationToken = default);
}
