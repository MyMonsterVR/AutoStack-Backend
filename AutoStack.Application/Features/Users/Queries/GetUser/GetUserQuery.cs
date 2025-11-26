using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Users;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

/// <summary>
/// Query to retrieve a user by their ID
/// </summary>
/// <param name="id">The ID of the user to retrieve</param>
public record GetUserQuery(Guid id) : IQuery<UserResponses>;