using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.Disable;

public record DisableTwoFactorCommand(
    Guid UserId,
    string Password,
    string TotpCode
) : ICommand<bool>;
