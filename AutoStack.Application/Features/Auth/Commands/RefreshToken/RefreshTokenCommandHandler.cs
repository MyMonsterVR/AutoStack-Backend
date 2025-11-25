using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IToken token,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
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