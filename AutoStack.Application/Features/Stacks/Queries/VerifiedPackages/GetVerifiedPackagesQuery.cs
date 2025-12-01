using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Queries.VerifiedPackages;

public record GetVerifiedPackagesQuery : IQuery<List<PackageResponse>>;