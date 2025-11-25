using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Username, string Password, string ConfirmPassword) : ICommand<bool>;