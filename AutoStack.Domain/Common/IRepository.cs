namespace AutoStack.Domain.Common;

public interface IRepository<TAggregate, TId>
    where TAggregate : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an aggregate by its ID.
    /// </summary>
    /// <param name="id">The aggregate ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aggregate if found; otherwise, null.</returns>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an aggregate by its ID.
    /// </summary>
    /// <param name="aggregate">The aggregate to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an aggregate exists by its ID.
    /// </summary>
    /// <param name="id">The aggregate ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the aggregate exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}