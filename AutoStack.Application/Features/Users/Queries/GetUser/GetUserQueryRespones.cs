namespace AutoStack.Application.Features.Users.Queries.GetUser;

public record GetUserQueryRespones(
    Guid Id,
    string Email,
    string Username,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
