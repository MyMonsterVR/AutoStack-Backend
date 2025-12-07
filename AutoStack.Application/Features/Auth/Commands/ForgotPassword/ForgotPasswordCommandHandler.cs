using System.Security.Cryptography;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    
    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }
    
    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Failure("User not found");
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.SetPasswordResetToken(token, DateTime.UtcNow.AddMinutes(15));
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Security,
                Message = "Request forgot password reset link",
                UserIdOverride = user.Id,
                UsernameOverride = user.Username
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failure
        }
        
        return Result<bool>.Success(true);
    }
}