using AutoStack.Application.Common.Exceptions;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.Login;

public class LoginCommandHandler(IAuthentication authentication, IUserRepository userRepository, IToken token) : ICommandHandler<LoginCommand, string>
{
    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await authentication.ValidateAuthenticationAsync(request.Username, request.Password, cancellationToken);

        if (userId == null || userId.Value == Guid.Empty)
        {
            throw new AuthenticationException("Invalid username or password");
        }
        
        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            throw new AuthenticationException("User not found");
        }
        
        
        var userToken = token.GenerateAccessToken(userId.Value, user.Username, user.Email);
        
        return Result<string>.Success(userToken);
    }
}