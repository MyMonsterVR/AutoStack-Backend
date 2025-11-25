using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface IStackRepository : IRepository<Stack, Guid>
{
    Task<Stack?> GetByIdWithInfoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stack>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stack>> GetAllAsync(CancellationToken cancellationToken = default);
}