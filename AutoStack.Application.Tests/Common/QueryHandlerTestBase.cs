using AutoStack.Domain.Repositories;
using Moq;

namespace AutoStack.Application.Tests.Common;

/// <summary>
/// Base class for testing query handlers with mocked repositories
/// </summary>
public abstract class QueryHandlerTestBase : IDisposable
{
    protected Mock<IStackRepository> MockStackRepository { get; }
    protected Mock<IPackageRepository> MockPackageRepository { get; }
    protected Mock<IUserRepository> MockUserRepository { get; }

    protected QueryHandlerTestBase()
    {
        MockStackRepository = new Mock<IStackRepository>();
        MockPackageRepository = new Mock<IPackageRepository>();
        MockUserRepository = new Mock<IUserRepository>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
