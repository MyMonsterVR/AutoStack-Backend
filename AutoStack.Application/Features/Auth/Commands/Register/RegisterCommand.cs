using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user account
/// </summary>
/// <param name="Email">The email address of the new user</param>
/// <param name="Username">The username for the new user</param>
/// <param name="Password">The password for the new user</param>
/// <param name="ConfirmPassword">The password confirmation</param>
public record RegisterCommand(string Email, string Username, string Password, string ConfirmPassword) : ICommand<RegisterResponse>;