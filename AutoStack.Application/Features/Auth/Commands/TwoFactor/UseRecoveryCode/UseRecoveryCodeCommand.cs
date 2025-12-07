using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.UseRecoveryCode;

public record UseRecoveryCodeCommand(
    string TwoFactorToken,
    string RecoveryCode
) : ICommand<LoginResponse>;
