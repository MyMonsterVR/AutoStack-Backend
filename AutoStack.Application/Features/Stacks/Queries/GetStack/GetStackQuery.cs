using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public record GetStackQuery(Guid Id) : ICommand<StackResponse>;