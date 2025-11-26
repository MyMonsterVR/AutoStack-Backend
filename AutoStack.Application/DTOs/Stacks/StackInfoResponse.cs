namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Response DTO containing package information within a stack
/// </summary>
/// <param name="PackageName">The name of the package</param>
/// <param name="PackageLink">The URL link to the package</param>
/// <param name="IsVerified">Whether the package has been verified by an administrator</param>
public record StackInfoResponse(string PackageName, string PackageLink, bool IsVerified);