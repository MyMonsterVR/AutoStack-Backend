using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.TwoFactor;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.ConfirmSetup;

public record ConfirmTwoFactorSetupCommand(
    string SetupToken,
    string TotpCode
) : ICommand<ConfirmTwoFactorSetupResponse>;
