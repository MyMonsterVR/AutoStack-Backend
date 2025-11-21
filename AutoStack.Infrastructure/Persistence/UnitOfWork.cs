using AutoStack.Application.Common.Interfaces;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Repositories;

namespace AutoStack.Infrastructure.Persistence;

public class UnitOfWork(
    ApplicationDbContext dbContext,
    IUserRepository? userRepository,
    IRefreshTokenRepository? refreshTokenRepository)
    : IUnitOfWork
{
    public IUserRepository Users => userRepository ?? new UserRepository(dbContext);
    public IRefreshTokenRepository RefreshTokenRepository => refreshTokenRepository ?? new RefreshTokenRepository(dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}