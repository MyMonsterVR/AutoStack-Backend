namespace AutoStack.Application.DTOs.Users;

public record UserResponses(
    Guid Id,
    string Email,
    string Username
);
