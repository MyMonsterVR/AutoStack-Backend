using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Auth.Commands.ResendVerificationEmail;

public record ResendVerificationEmailCommand(Guid UserId) : ICommand<bool>;
