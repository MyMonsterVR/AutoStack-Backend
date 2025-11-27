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

    private static IResult GetCliVersion()
    {
        // Update this version number when you release a new CLI version
        return Results.Ok(new CliVersionResponse { Version = "1.0.0" });
    }
}

public record CliVersionResponse
{
    public string Version { get; init; } = string.Empty;
}
