using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.Login;

public class LoginCommandHandler(IAuthentication authentication, IUserRepository userRepository, IToken token) : ICommandHandler<LoginCommand, string>
{
    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await authentication.ValidateAuthenticationAsync(request.Username.ToLower(), request.Password, cancellationToken);

        if (userId == null || userId.Value == Guid.Empty)
        {
            return Result<string>.Failure("Invalid username or password");
        }

        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result<string>.Failure("User not found");
        }

        var userToken = token.GenerateAccessToken(userId.Value, user.Username, user.Email);

        return Result<string>.Success(userToken);
    }
}