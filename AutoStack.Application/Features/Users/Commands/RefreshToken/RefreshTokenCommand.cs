using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<string>;