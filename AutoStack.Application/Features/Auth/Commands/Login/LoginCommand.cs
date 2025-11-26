using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user with username and password
/// </summary>
/// <param name="Username">The username of the user</param>
/// <param name="Password">The password of the user</param>
public record LoginCommand(string Username, string Password) : ICommand<LoginResponse>;