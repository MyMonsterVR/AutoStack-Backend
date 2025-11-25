using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IAuthentication authentication,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IToken token,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await authentication.ValidateAuthenticationAsync(request.Username.ToLower(), request.Password, cancellationToken);

        if (userId == null || userId.Value == Guid.Empty)
        {
            return Result<LoginResponse>.Failure("Invalid username or password");
        }

        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("User not found");
        }

        var generatedToken = token.GenerateAccessToken(userId.Value, user.Username, user.Email);
        var userToken = token.GenerateRefreshToken(userId.Value);

        await refreshTokenRepository.AddAsync(userToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
             AccessToken = generatedToken,
             RefreshToken = userToken.Token,
        };
        
        return Result<LoginResponse>.Success(response);
    }
}