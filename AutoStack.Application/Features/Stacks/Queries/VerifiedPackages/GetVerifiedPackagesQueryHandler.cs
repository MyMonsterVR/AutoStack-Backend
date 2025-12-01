using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.VerifiedPackages;

public class GetVerifiedPackagesQueryHandler : IQueryHandler<GetVerifiedPackagesQuery, List<PackageResponse>>
{
    private readonly IStackRepository _stackRepository;
    
    public GetVerifiedPackagesQueryHandler(IStackRepository stackRepository)
    {
        _stackRepository = stackRepository;
    }
    
    public async Task<Result<List<PackageResponse>>> Handle(GetVerifiedPackagesQuery request, CancellationToken cancellationToken)
    {
        var verifiedPackages = await _stackRepository.GetVerifiedPackagesAsync(cancellationToken);

        var packages = verifiedPackages.Select(
            verifiedPackage => new PackageResponse(verifiedPackage.Name, verifiedPackage.Link, verifiedPackage.IsVerified)
        ).ToList();


        return Result<List<PackageResponse>>.Success(packages);
    }
}