using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, bool>
{
    public async Task<Result<bool>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExists(request.Email.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Email already exists");
        }
        
        if (await userRepository.UsernameExists(request.Username.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Username already exists");
        }
        
        var user = User.CreateUser(request.Email.ToLower(), request.Username.ToLower());

        var passwordHashed = passwordHasher.HashPassword(request.Password);
        user.SetPassword(passwordHashed);
        
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}