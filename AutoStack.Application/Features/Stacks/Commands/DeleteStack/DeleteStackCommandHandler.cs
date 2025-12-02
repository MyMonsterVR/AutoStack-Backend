using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.DeleteStack;

public class DeleteStackCommandHandler : ICommandHandler<DeleteStackCommand, bool>
{
    private readonly IStackRepository _stackRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;

    public DeleteStackCommandHandler(IStackRepository stackRepository, IUnitOfWork unitOfWork, IAuditLogService auditLogService, IUserRepository userRepository)
    {
        _stackRepository = stackRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _userRepository = userRepository;
    }
    
    public async Task<Result<bool>> Handle(DeleteStackCommand request, CancellationToken cancellationToken)
    {
        // Load stack with packages for detailed logging
        var stack = await _stackRepository.GetByIdWithInfoAsync(request.StackId, cancellationToken);
        if (stack == null)
        {
            return Result<bool>.Failure("Stack not found");
        }

        // Capture stack details before deletion
        var stackName = stack.Name;
        var stackType = stack.Type;
        var stackUserId = stack.UserId;
        var packageCount = stack.Packages?.Count ?? 0;
        var packageNames = stack.Packages?.Select(p => p.Package.Name).ToList() ?? new List<string>();
        var downloads = stack.Downloads;

        // Get user for audit log
        var user = await _userRepository.GetByIdAsync(stackUserId, cancellationToken);
        var username = user?.Username ?? "Unknown";

        await _stackRepository.DeleteAsync(stack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log stack deletion
        try
        {
            var additionalData = new Dictionary<string, object>
            {
                ["StackId"] = request.StackId,
                ["StackName"] = stackName,
                ["StackType"] = stackType,
                ["PackageCount"] = packageCount,
                ["Packages"] = packageNames,
                ["Downloads"] = downloads
            };

            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Stack,
                Message = $"Stack '{stackName}' deleted ({packageCount} package(s), {downloads} download(s))",
                UserIdOverride = stackUserId,
                UsernameOverride = username,
                AdditionalData = additionalData
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failures
        }

        return Result<bool>.Success(true);
    }
}