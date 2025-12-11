namespace AutoStack.Application.DTOs.Login;

/// <summary>
/// Response DTO for user registration containing the newly created user's ID.
/// </summary>
/// <param name="UserId">The unique identifier of the newly registered user.</param>
public record RegisterResponse(
    Guid UserId
);
