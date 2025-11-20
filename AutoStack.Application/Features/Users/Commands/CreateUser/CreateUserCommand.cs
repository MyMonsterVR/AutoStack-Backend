using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email, string Username, string Password) : ICommand<int>;