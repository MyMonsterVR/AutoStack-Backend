using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.VerifyLogin;

public record VerifyTwoFactorLoginCommand(
    string TwoFactorToken,
    string TotpCode
) : ICommand<LoginResponse>;
