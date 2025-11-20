using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid id) : IQuery<UserRespones>;