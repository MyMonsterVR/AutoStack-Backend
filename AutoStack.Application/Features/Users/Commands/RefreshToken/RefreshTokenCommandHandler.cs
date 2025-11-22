using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository) : ICommandHandler<RefreshTokenCommand, string>
{
    public async Task<Result<string>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var requestTokenData = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (requestTokenData == null)
        {
            return Result<string>.Failure("RefreshToken not found");
        }
        
        DateTime expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(requestTokenData.ExpiresAt).UtcDateTime;
        
        var isExpired = expirationDateTime < DateTime.UtcNow;
        if (isExpired)
        {
            return Result<string>.Failure("RefreshToken is expired");
        }
        
        return Result<string>.Success("RefreshToken has been refreshed");
    }
}