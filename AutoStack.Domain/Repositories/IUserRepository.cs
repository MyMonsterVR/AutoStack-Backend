using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<int> CountStacksByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountTemplatesByUserId(Guid userId, CancellationToken cancellationToken = default);
}