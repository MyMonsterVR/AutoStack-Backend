using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;