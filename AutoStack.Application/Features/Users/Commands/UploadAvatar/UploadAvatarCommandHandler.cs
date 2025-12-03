using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandler : ICommandHandler<UploadAvatarCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;

    public UploadAvatarCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<UserResponse>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
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

            var oldAvatarUrl = user.AvatarUrl;

            var avatarUrl = await _fileStorageService.UploadAvatarAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            user.SetAvatarUrl(avatarUrl);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Delete old avatar if it exists and is from our storage
            if (!string.IsNullOrWhiteSpace(oldAvatarUrl) && oldAvatarUrl.Contains("/uploads/avatars/"))
            {
                try
                {
                    await _fileStorageService.DeleteAvatarAsync(oldAvatarUrl, cancellationToken);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }

            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Information,
                    Category = LogCategory.User,
                    Message = "User avatar uploaded",
                    UserIdOverride = user.Id,
                    UsernameOverride = user.Username,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["OldAvatarUrl"] = oldAvatarUrl ?? "null",
                        ["NewAvatarUrl"] = avatarUrl,
                        ["FileName"] = request.FileName,
                        ["FileSize"] = request.FileSize
                    }
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            var userResponse = new UserResponse(user.Id, user.Email, user.Username, user.AvatarUrl);

            return Result<UserResponse>.Success(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return Result<UserResponse>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<UserResponse>.Failure(ex.Message);
        }
    }
}
