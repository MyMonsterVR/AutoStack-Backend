using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;

namespace AutoStack.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(User aggregate, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(aggregate, cancellationToken);
    }

    public Task UpdateAsync(User aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.UpdateTimestamp();
        dbContext.Users.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FindAsync([id], cancellationToken) != null;
    }

    public Task<int> CountStacksByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountTemplatesByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}