namespace AutoStack.Application.Features.Users.Queries.GetUser;

public record UserRespones(
    Guid Id,
    string Email,
    string Username,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
