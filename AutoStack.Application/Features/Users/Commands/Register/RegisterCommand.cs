using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Users.Commands.Register;

public record RegisterCommand(string Email, string Username, string Password, string ConfirmPassword) : ICommand<bool>;