using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles the registration command by creating a new user account
/// </summary>
public class RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterCommand, bool>
{
    /// <summary>
    /// Processes the registration request by validating the data and creating a new user
    /// </summary>
    /// <param name="request">The registration command containing user details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result indicating success or failure with an error message</returns>
    public async Task<Result<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExists(request.Email.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Email already exists");
        }
        
        if (await userRepository.UsernameExists(request.Username.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Username already exists");
        }
        
        if (request.Password != request.ConfirmPassword)
        {
            return Result<bool>.Failure("Passwords do not match");
        }
        
        var user = User.CreateUser(request.Email.ToLower(), request.Username.ToLower());

        var passwordHashed = passwordHasher.HashPassword(request.Password);
        user.SetPassword(passwordHashed);
        
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}