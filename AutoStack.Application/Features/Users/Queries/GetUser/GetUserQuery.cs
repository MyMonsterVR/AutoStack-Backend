using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Users;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid id) : IQuery<UserResponses>;