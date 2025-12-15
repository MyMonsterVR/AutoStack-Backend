using AutoStack.Application.DTOs.Cli;
using AutoStack.Application.Features.Cli.Queries.GetCliVersion;
using AutoStack.Application.Features.Stacks.Commands.TrackDownload;
using MediatR;

namespace AutoStack.Presentation.Endpoints.Cli;

public static class CliEndpoints
{
    public static void MapCliEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/cli");

        group.MapGet("version", GetCliVersion)
            .WithName("GetCliVersion")
            .WithTags("CLI")
            .Produces<CliVersionResponse>();

        group.MapPost("track/{stackId:guid}", TrackDownload)
            .WithName("TrackStackDownload")
            .WithTags("CLI")
            .WithSummary("Track a stack download (CLI only - supports both authenticated and unauthenticated requests)")
            .RequireRateLimiting("cli-track")
            .Produces(200)
            .Produces(400)
            .Produces(404)
            .Produces(429);
    }

    private static async Task<IResult> GetCliVersion(ISender sender, CancellationToken cancellationToken)
    {
        var query = new GetCliVersionQuery();
        var result = await sender.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.NotFound(new { message = result.Message });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> TrackDownload(Guid stackId, ISender sender, CancellationToken cancellationToken)
    {
        var command = new TrackDownloadCommand(stackId);
        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.NotFound(new { error = result.Message });
        }

        return Results.Ok(new { message = "Download tracked successfully", downloads = result.Value.Downloads });
    }
}
