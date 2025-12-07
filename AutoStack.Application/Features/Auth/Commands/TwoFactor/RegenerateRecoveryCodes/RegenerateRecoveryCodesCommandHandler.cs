using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.TwoFactor;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.RegenerateRecoveryCodes;

public class RegenerateRecoveryCodesCommandHandler : ICommandHandler<RegenerateRecoveryCodesCommand, RegenerateRecoveryCodesResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecoveryCodeGenerator _recoveryCodeGenerator;
    private readonly IAuditLogService _auditLogService;

    public RegenerateRecoveryCodesCommandHandler(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository,
        ITotpService totpService,
        IEncryptionService encryptionService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IRecoveryCodeGenerator recoveryCodeGenerator,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _recoveryCodeGenerator = recoveryCodeGenerator;
        _auditLogService = auditLogService;
    }

    public async Task<Result<RegenerateRecoveryCodesResponse>> Handle(RegenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<RegenerateRecoveryCodesResponse>.Failure("User not found");
        }

        if (!user.TwoFactorEnabled)
        {
            return Result<RegenerateRecoveryCodesResponse>.Failure("Two-factor authentication is not enabled");
        }

        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return Result<RegenerateRecoveryCodesResponse>.Failure("Invalid password");
        }

        if (!string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            var secretKey = _encryptionService.Decrypt(user.TwoFactorSecretKey);
            var codeValid = _totpService.ValidateCode(secretKey, request.TotpCode);
            if (!codeValid)
            {
                return Result<RegenerateRecoveryCodesResponse>.Failure("Invalid verification code");
            }
        }

        // Delete existing recovery codes for user
        await _recoveryCodeRepository.DeleteAllByUserIdAsync(user.Id, cancellationToken);

        var recoveryCodes = _recoveryCodeGenerator.GenerateCodes();

        foreach (var code in recoveryCodes)
        {
            var hashedCode = _recoveryCodeGenerator.HashCode(code);
            var recoveryCode = RecoveryCode.Create(user.Id, hashedCode);
            await _recoveryCodeRepository.AddAsync(recoveryCode, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Security,
                Message = "Recovery codes regenerated",
                UserIdOverride = user.Id,
                UsernameOverride = user.Username
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failures
        }

        // Format recovery codes for display
        var formattedCodes = recoveryCodes.Select(c => _recoveryCodeGenerator.FormatForDisplay(c)).ToList();

        var response = new RegenerateRecoveryCodesResponse
        {
            RecoveryCodes = formattedCodes
        };

        return Result<RegenerateRecoveryCodesResponse>.Success(response);
    }
}
