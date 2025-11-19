using AutoStack.Application.Common.Interfaces;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using AutoStack.Infrastructure.Repositories;

namespace AutoStack.Infrastructure;

public class UnitOfWork(ApplicationDbContext dbContext, IUserRepository? userRepository) : IUnitOfWork
{
    public IUserRepository Users => userRepository ?? new UserRepository(dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}