using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface ICliVersionRepository
{
    Task<CliVersion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
