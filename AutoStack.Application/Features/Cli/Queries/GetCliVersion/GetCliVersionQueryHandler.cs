using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Cli;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Cli.Queries.GetCliVersion;

public class GetCliVersionQueryHandler : IQueryHandler<GetCliVersionQuery, CliVersionResponse>
{
    private readonly ICliVersionRepository _cliVersionRepository;

    public GetCliVersionQueryHandler(ICliVersionRepository cliVersionRepository)
    {
        _cliVersionRepository = cliVersionRepository;
    }

    public async Task<Result<CliVersionResponse>> Handle(GetCliVersionQuery request, CancellationToken cancellationToken)
    {
        var cliVersion = await _cliVersionRepository.GetByIdAsync(1, cancellationToken);

        if (cliVersion == null)
        {
            return Result<CliVersionResponse>.Failure("CLI version not configured");
        }

        var response = new CliVersionResponse
        {
            Version = cliVersion.Version
        };

        return Result<CliVersionResponse>.Success(response);
    }
}
