namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Response DTO containing package information within a stack
/// </summary>
/// <param name="Name">The name of the package</param>
/// <param name="Link">The URL link to the package</param>
/// <param name="IsVerified">Whether the package has been verified by an administrator</param>
public record PackageResponse(string Name, string Link, bool IsVerified);