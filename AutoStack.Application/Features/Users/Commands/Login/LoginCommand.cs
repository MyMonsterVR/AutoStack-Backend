using System.Windows.Input;
using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Users.Commands.Login;

public record LoginCommand(string Username, string Password) : ICommand<string>;