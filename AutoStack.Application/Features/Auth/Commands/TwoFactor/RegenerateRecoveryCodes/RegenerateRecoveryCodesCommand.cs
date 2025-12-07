using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.TwoFactor;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.RegenerateRecoveryCodes;

public record RegenerateRecoveryCodesCommand(
    Guid UserId,
    string Password,
    string TotpCode
) : ICommand<RegenerateRecoveryCodesResponse>;
