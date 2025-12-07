using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.TwoFactor;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.BeginSetup;

public record BeginTwoFactorSetupCommand(Guid UserId) : ICommand<BeginTwoFactorSetupResponse>;
