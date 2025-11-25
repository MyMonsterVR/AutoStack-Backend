using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

public interface IPackageRepository : IRepository<Package, Guid>
{
    Task<Package?> GetByLinkAsync(string link, CancellationToken cancellationToken = default);
    Task<IEnumerable<Package>> GetVerifiedPackagesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByLinkAsync(string link, CancellationToken cancellationToken = default);
}
