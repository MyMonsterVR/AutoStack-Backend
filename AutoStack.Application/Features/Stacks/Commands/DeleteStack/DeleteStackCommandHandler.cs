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

    public DeleteStackCommandHandler(IStackRepository stackRepository, IUnitOfWork unitOfWork, IAuditLogService auditLogService, IUserRepository userRepository)
    {
        _stackRepository = stackRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }
    
    public async Task<Result<bool>> Handle(DeleteStackCommand request, CancellationToken cancellationToken)
    {
        // Load stack with packages for detailed logging
        var stackInfo = await _stackRepository.GetByIdWithInfoAsync(request.StackId, cancellationToken);
        if (stackInfo == null)
        {
            return Result<bool>.Failure("Stack not found");
        }

        // Capture stack details before deletion
        var stackName = stackInfo.Name;
        var stackType = stackInfo.Type;
        var stackUserId = stackInfo.UserId;
        var packageCount = stackInfo.Packages.Count;
        var packageNames = stackInfo.Packages?.Select(p => p.Package.Name).ToList() ?? new List<string>();
        var downloads = stackInfo.Downloads;
        var username = stackInfo.User.Username;

        // Load the stack for deletion (without includes to avoid tracking conflicts)
        var stack = await _stackRepository.GetByIdAsync(request.StackId, cancellationToken);
        if (stack == null)
        {
            return Result<bool>.Failure("Stack not found");
        }

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