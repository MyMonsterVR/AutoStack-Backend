using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Login;

namespace AutoStack.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a refresh token
/// </summary>
/// <param name="RefreshToken">The refresh token to use for generating new tokens</param>
public record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;