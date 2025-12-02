using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;
using AutoStack.Domain.Enums;

namespace AutoStack.Application.Features.Stacks.Commands.TrackDownload;

/// <summary>
/// Handles the track download command by incrementing the stack's download counter
/// </summary>
public class TrackDownloadCommandHandler : ICommandHandler<TrackDownloadCommand, StackResponse>
{
    private readonly IStackRepository _stackRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public TrackDownloadCommandHandler(
        IStackRepository stackRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _stackRepository = stackRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Processes the track download request by incrementing the stack's download count
    /// </summary>
    /// <param name="request">The track download command containing the stack ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the updated stack response on success, or an error message on failure</returns>
    public async Task<Result<StackResponse>> Handle(TrackDownloadCommand request, CancellationToken cancellationToken)
    {
        // Use GetByIdAsync to get a tracked entity (GetByIdWithInfoAsync uses AsNoTracking)
        var stack = await _stackRepository.GetByIdAsync(request.StackId, cancellationToken);

        if (stack == null)
        {
            return Result<StackResponse>.Failure("Stack not found");
        }

        stack.IncrementDownloads();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var stackWithInfo = await _stackRepository.GetByIdWithInfoAsync(request.StackId, cancellationToken);
        if (stackWithInfo == null)
        {
            return Result<StackResponse>.Failure("Stack not found");
        }

        // Log the download event
        await _auditLogService.LogAsync(new AuditLogRequest
        {
            Level = LogLevel.Information,
            Category = LogCategory.Stack,
            Message = $"Stack '{stackWithInfo.Name}' (ID: {stackWithInfo.Id}) downloaded via CLI. New download count: {stackWithInfo.Downloads}"
        }, cancellationToken);

        var user = await _userRepository.GetByIdAsync(stackWithInfo.UserId, cancellationToken);
        if (user == null)
        {
            return Result<StackResponse>.Failure("User not found");
        }

        var packages = stackWithInfo.Packages
            .Select(si => new PackageResponse(si.Package.Name, si.Package.Link, si.Package.IsVerified))
            .ToList();

        var response = new StackResponse
        {
            Id            = stackWithInfo.Id,
            Name          = stackWithInfo.Name,
            Description   = stackWithInfo.Description,
            Type          = Enum.Parse<StackTypeResponse>(stackWithInfo.Type),
            Downloads     = stackWithInfo.Downloads,
            Packages      = packages,
            UserId        = stackWithInfo.UserId,
            Username      = user.Username,
            UserAvatarUrl = user.AvatarUrl
        };

        return Result<StackResponse>.Success(response);
    }
}
