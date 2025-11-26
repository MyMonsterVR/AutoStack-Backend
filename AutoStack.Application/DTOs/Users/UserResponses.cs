namespace AutoStack.Application.DTOs.Users;

/// <summary>
/// Response DTO containing user information
/// </summary>
/// <param name="Id">The unique identifier of the user</param>
/// <param name="Email">The email address of the user</param>
/// <param name="Username">The username of the user</param>
public record UserResponses(
    Guid Id,
    string Email,
    string Username
);
