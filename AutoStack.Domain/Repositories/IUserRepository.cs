using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExists(string username, CancellationToken cancellationToken = default);
    Task<int> CountStacksByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountTemplatesByUserId(Guid userId, CancellationToken cancellationToken = default);
}