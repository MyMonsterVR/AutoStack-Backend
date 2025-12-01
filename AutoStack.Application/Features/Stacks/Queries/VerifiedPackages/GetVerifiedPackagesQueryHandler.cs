using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.VerifiedPackages;

public class GetVerifiedPackagesQueryHandler : IQueryHandler<GetVerifiedPackagesQuery, List<PackageResponse>>
{
    private readonly IPackageRepository _packageRepository;
    
    public GetVerifiedPackagesQueryHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }
    
    public async Task<Result<List<PackageResponse>>> Handle(GetVerifiedPackagesQuery request, CancellationToken cancellationToken)
    {
        var verifiedPackages = await _packageRepository.GetVerifiedPackagesAsync(cancellationToken);

        var packages = verifiedPackages.Select(
            verifiedPackage => new PackageResponse(verifiedPackage.Name, verifiedPackage.Link, verifiedPackage.IsVerified)
        ).ToList();


        return Result<List<PackageResponse>>.Success(packages);
    }
}