using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Commands.CreateStack;

/// <summary>
/// Represents a package to be included in a stack
/// </summary>
/// <param name="Name">The name of the package</param>
/// <param name="Link">The URL link to the package</param>
public record PackageInput(string Name, string Link);

/// <summary>
/// Command to create a new technology stack with packages
/// </summary>
/// <param name="Name">The name of the stack</param>
/// <param name="Description">The description of the stack</param>
/// <param name="Type">The type of the stack (frontend, backend, fullstack)</param>
/// <param name="Packages">The list of packages to include in the stack</param>
/// <param name="UserId">The ID of the user creating the stack</param>
public record CreateStackCommand(
    string Name,
    string Description,
    StackType Type,
    List<PackageInput> Packages,
    Guid? UserId = null
) : ICommand<StackResponse>;