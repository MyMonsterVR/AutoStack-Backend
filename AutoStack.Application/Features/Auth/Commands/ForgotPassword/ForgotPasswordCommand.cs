using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand<bool>;