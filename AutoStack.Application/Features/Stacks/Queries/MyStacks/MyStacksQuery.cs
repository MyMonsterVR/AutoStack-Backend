using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Queries.MyStacks;

public record MyStacksQuery(Guid? UserId) : IQuery<IEnumerable<StackResponse>>;