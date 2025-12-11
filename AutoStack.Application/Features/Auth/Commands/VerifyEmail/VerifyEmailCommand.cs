using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(Guid UserId, string Code) : ICommand<bool>;
