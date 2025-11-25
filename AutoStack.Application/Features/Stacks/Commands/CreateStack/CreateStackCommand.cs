using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Commands.CreateStack;

public record PackageInput(string Name, string Link);

public record CreateStackCommand(
    string Name,
    string Description,
    StackType Type,
    List<PackageInput> Packages,
    Guid? UserId = null
) : ICommand<StackResponse>;