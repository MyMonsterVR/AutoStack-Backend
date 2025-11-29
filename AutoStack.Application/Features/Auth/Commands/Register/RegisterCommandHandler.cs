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
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes the registration request by validating the data and creating a new user
    /// </summary>
    /// <param name="request">The registration command containing user details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result indicating success or failure with an error message</returns>
    public async Task<Result<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExists(request.Email.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Email already exists");
        }
        
        if (await _userRepository.UsernameExists(request.Username.ToLower(), cancellationToken))
        {
            return Result<bool>.Failure("Username already exists");
        }
        
        if (request.Password != request.ConfirmPassword)
        {
            return Result<bool>.Failure("Passwords do not match");
        }
        
        var user = User.CreateUser(request.Email.ToLower(), request.Username.ToLower());

        var passwordHashed = _passwordHasher.HashPassword(request.Password);
        user.SetPassword(passwordHashed);
        
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}