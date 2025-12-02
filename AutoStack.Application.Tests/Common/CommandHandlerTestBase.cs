using AutoStack.Application.Common.Interfaces;
using AutoStack.Domain.Repositories;
using Moq;

namespace AutoStack.Application.Tests.Common;

/// <summary>
/// Base class for testing command handlers with mocked repositories
/// </summary>
public abstract class CommandHandlerTestBase : IDisposable
{
    protected Mock<IStackRepository> MockStackRepository { get; }
    protected Mock<IPackageRepository> MockPackageRepository { get; }
    protected Mock<IUserRepository> MockUserRepository { get; }
    protected Mock<IRefreshTokenRepository> MockRefreshTokenRepository { get; }
    protected Mock<IUnitOfWork> MockUnitOfWork { get; }
    protected Mock<IAuditLogService> MockAuditLog { get; }

    protected CommandHandlerTestBase()
    {
        MockStackRepository = new Mock<IStackRepository>();
        MockPackageRepository = new Mock<IPackageRepository>();
        MockUserRepository = new Mock<IUserRepository>();
        MockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockAuditLog = new Mock<IAuditLogService>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
