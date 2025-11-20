using AutoStack.Application.Common.Interfaces;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid id) : IQuery<GetUserQueryRespones>;