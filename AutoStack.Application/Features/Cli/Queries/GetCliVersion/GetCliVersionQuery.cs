using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Cli;

namespace AutoStack.Application.Features.Cli.Queries.GetCliVersion;

public record GetCliVersionQuery : IQuery<CliVersionResponse>;
