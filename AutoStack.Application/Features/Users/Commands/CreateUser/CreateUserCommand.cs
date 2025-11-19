using AutoStack.Application.Common.Interfaces;

namespace AutoStack.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email, string Username, string Password) : ICommand<int>;