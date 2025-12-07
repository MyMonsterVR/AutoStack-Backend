using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogService auditLogService,
        IPasswordHasher passwordHasher
    )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogService = auditLogService;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);

        if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return Result<bool>.Failure($"Invalid or expired token");
        }
        
        var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        user.SetPassword(newHashedPassword);
        user.ClearPasswordResetToken();
        
        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Security,
                Message = "User password has been reset",
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
}