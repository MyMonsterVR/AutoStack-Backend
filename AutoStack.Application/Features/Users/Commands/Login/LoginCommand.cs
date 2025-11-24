using System.Windows.Input;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Users.Commands.Login;

public record LoginCommand(string Username, string Password) : ICommand<LoginResponse>;