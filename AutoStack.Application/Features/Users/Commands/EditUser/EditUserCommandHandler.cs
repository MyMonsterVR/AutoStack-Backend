using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.EditUser;

public class EditUserCommandHandler : ICommandHandler<EditUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public EditUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserResponse>> Handle(EditUserCommand request, CancellationToken cancellationToken)
    {
        
        try
        {
            if (!request.UserId.HasValue)
            {
                return Result<UserResponse>.Failure("User ID is required");
            }
        
            var user = await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken);

            if (user == null)
            {
                return Result<UserResponse>.Failure("User not found.");
            }

            if (user.Username != request.Username)
            {
                user.SetUsername(request.Username);
            }

            if (user.Email != request.Email)
            {
                user.SetEmail(request.Email);
            }

            if (user.AvatarUrl != request.AvatarUrl)
            {
                user.SetAvatarUrl(request.AvatarUrl);
            }
        
            if (
                !string.IsNullOrWhiteSpace(request.CurrentPassword)
                && !string.IsNullOrWhiteSpace(request.NewPassword)
                && !string.IsNullOrWhiteSpace(request.ConfirmNewPassword)
            )
            {
                var isPasswordValid = _passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash);
                if (!isPasswordValid)
                {
                    return Result<UserResponse>.Failure("Current password is invalid");
                }

                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return  Result<UserResponse>.Failure("Passwords do not match");
                }
                
                var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
                user.SetPassword(newHashedPassword);
            }
        
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userResponse = new UserResponse(user.Id, user.Email, user.Username, user.AvatarUrl);

            return Result<UserResponse>.Success(userResponse);
        }
        catch(ArgumentException ae)
        {
            return Result<UserResponse>.Failure(ae.Message);
        }
    }
}