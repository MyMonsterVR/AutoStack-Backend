using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Users;

namespace AutoStack.Application.Features.Users.Commands.EditUser;

public record EditUserCommand(
    string Username,
    string Email,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword,
    Guid? UserId
) : ICommand<UserResponse>;