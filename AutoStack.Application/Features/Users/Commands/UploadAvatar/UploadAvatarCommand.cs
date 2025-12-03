using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Users;

namespace AutoStack.Application.Features.Users.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    Guid? UserId
) : ICommand<UserResponse>;
