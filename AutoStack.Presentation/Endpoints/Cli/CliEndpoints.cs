using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
    }

    private static async Task<IResult> GetCliVersion(ApplicationDbContext dbContext)
    {
        var cliVersion = await dbContext.CliVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.Id == 1);

        if (cliVersion == null)
        {
            return Results.NotFound(new { message = "CLI version not configured" });
        }

        return Results.Ok(new CliVersionResponse { Version = cliVersion.Version });
    }
}

public record CliVersionResponse
{
    public string Version { get; init; } = string.Empty;
}
