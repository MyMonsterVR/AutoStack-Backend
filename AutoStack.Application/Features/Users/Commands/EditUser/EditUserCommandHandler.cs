using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.EditUser;

public class EditUserCommandHandler : ICommandHandler<EditUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLogService;

    public EditUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
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

            var changedFields = new List<string>();
            var additionalData = new Dictionary<string, object>();

            if (user.Username != request.Username)
            {
                additionalData["OldUsername"] = user.Username;
                additionalData["NewUsername"] = request.Username;
                user.SetUsername(request.Username);
                changedFields.Add("Username");
            }

            if (user.Email != request.Email)
            {
                additionalData["OldEmail"] = MaskEmail(user.Email);
                additionalData["NewEmail"] = MaskEmail(request.Email);
                user.SetEmail(request.Email);
                changedFields.Add("Email");
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
                    // Log failed password change attempt
                    try
                    {
                        await _auditLogService.LogAsync(new AuditLogRequest
                        {
                            Level = LogLevel.Warning,
                            Category = LogCategory.Security,
                            Message = "Failed password change attempt - incorrect current password",
                            UserIdOverride = user.Id,
                            UsernameOverride = user.Username
                        }, cancellationToken);
                    }
                    catch
                    {
                        // Ignore logging failures
                    }

                    return Result<UserResponse>.Failure("Current password is invalid");
                }

                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return  Result<UserResponse>.Failure("Passwords do not match");
                }

                var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
                user.SetPassword(newHashedPassword);
                changedFields.Add("Password");
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log successful profile update
            if (changedFields.Count > 0)
            {
                try
                {
                    additionalData["ChangedFields"] = changedFields;
                    await _auditLogService.LogAsync(new AuditLogRequest
                    {
                        Level = LogLevel.Information,
                        Category = LogCategory.User,
                        Message = $"User profile updated: {string.Join(", ", changedFields)}",
                        UserIdOverride = user.Id,
                        UsernameOverride = user.Username,
                        AdditionalData = additionalData
                    }, cancellationToken);
                }
                catch
                {
                    // Ignore logging failures
                }
            }

            var userResponse = new UserResponse(user.Id, user.Email, user.Username, user.AvatarUrl, user.EmailVerified);

            return Result<UserResponse>.Success(userResponse);
        }
        catch(ArgumentException ae)
        {
            return Result<UserResponse>.Failure(ae.Message);
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "null";

        var parts = email.Split('@');
        if (parts.Length != 2) return "invalid";

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}***@{domain}";

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }
}