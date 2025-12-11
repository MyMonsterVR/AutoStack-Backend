using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure("User not found");
        }

        if (user.EmailVerified)
        {
            return Result<bool>.Failure("Email is already verified");
        }

        if (string.IsNullOrEmpty(user.EmailVerificationCode))
        {
            return Result<bool>.Failure("No verification code found. Please request a new code.");
        }

        if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
        {
            return Result<bool>.Failure("Verification code has expired. Please request a new code.");
        }

        if (user.EmailVerificationCode != request.Code)
        {
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = "Invalid email verification code attempt",
                    UserIdOverride = user.Id,
                    UsernameOverride = user.Username
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<bool>.Failure("Invalid verification code");
        }

        user.VerifyEmail();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log successful verification
        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Authentication,
                Message = "Email verified successfully",
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
