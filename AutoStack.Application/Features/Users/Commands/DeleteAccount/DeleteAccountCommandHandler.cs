using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Commands.DeleteAccount;

public class DeleteAccountCommandHandler : ICommandHandler<DeleteAccountCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    public DeleteAccountCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }
    
    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.UserId.HasValue)
            {
                return Result<bool>.Failure("User ID is required");
            }
            
            var user = await _userRepository.GetByIdAsync(request.UserId.Value, cancellationToken);

            if (user == null)
            {
                return Result<bool>.Failure("User not found.");
            }
            
            await _userRepository.DeleteAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Information,
                    Category = LogCategory.User,
                    Message = "Account deleted",
                    UserIdOverride = user.Id,
                    UsernameOverride = user.Username
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}