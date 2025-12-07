using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.Disable;

public class DisableTwoFactorCommandHandler : ICommandHandler<DisableTwoFactorCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public DisableTwoFactorCommandHandler(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository,
        ITotpService totpService,
        IEncryptionService encryptionService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<Result<bool>> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Failure("User not found");
        }

        if (!user.TwoFactorEnabled)
        {
            return Result<bool>.Failure("Two-factor authentication is not enabled");
        }

        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return Result<bool>.Failure("Invalid password");
        }

        if (!string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            var secretKey = _encryptionService.Decrypt(user.TwoFactorSecretKey);
            var codeValid = _totpService.ValidateCode(secretKey, request.TotpCode);
            if (!codeValid)
            {
                return Result<bool>.Failure("Invalid verification code");
            }
        }

        user.DisableTwoFactorAuthentication();

        // Delete all recovery codes for user
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(user.Id, cancellationToken);
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Warning,
                Category = LogCategory.Security,
                Message = "Two-factor authentication disabled",
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
