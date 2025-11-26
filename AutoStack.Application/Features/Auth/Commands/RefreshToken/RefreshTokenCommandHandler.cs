using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handles the refresh token command by validating the refresh token and generating new authentication tokens
/// </summary>
public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IToken token,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    /// <summary>
    /// Processes the refresh token request by validating the token and returning new access and refresh tokens
    /// </summary>
    /// <param name="request">The refresh token command</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing new tokens on success, or an error message on failure</returns>
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenData = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshTokenData == null)
        {
            return Result<LoginResponse>.Failure("RefreshToken not found");
        }
        
        var refreshTokenExpirationDateTime = DateTimeOffset.FromUnixTimeSeconds(refreshTokenData.ExpiresAt).UtcDateTime;
        
        var isExpired = refreshTokenExpirationDateTime < DateTime.UtcNow;
        if (isExpired)
        {
            return Result<LoginResponse>.Failure("RefreshToken has expired");
        }
        
        var user = await userRepository.GetByIdAsync(refreshTokenData.UserId, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("User not found");
        }

        var newAccessToken = token.GenerateAccessToken(user.Id, user.Username, user.Email);
        var newRefreshToken = token.GenerateRefreshToken(user.Id);
        
        await refreshTokenRepository.DeleteAsync(refreshTokenData, cancellationToken);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var response = new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
        };
        
        return Result<LoginResponse>.Success(response);
    }
}