namespace AutoStack.Application.DTOs.Cli;

public record CliVersionResponse
{
    public string Version { get; init; } = string.Empty;
}
