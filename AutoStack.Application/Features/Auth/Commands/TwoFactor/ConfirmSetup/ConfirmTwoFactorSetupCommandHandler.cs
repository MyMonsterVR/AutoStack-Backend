using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.TwoFactor;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.ConfirmSetup;

public class ConfirmTwoFactorSetupCommandHandler : ICommandHandler<ConfirmTwoFactorSetupCommand, ConfirmTwoFactorSetupResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly IToken _token;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecoveryCodeGenerator _recoveryCodeGenerator;
    private readonly IAuditLogService _auditLogService;

    public ConfirmTwoFactorSetupCommandHandler(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository,
        ITotpService totpService,
        IEncryptionService encryptionService,
        IToken token,
        IUnitOfWork unitOfWork,
        IRecoveryCodeGenerator recoveryCodeGenerator,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _token = token;
        _unitOfWork = unitOfWork;
        _recoveryCodeGenerator = recoveryCodeGenerator;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ConfirmTwoFactorSetupResponse>> Handle(ConfirmTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        var tokenData = _token.VerifySetupToken(request.SetupToken);
        if (tokenData == null)
        {
            return Result<ConfirmTwoFactorSetupResponse>.Failure("Invalid or expired setup token");
        }

        var (userId, secretKey) = tokenData.Value;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<ConfirmTwoFactorSetupResponse>.Failure("User not found");
        }

        var codeValid = _totpService.ValidateCode(secretKey, request.TotpCode);
        if (!codeValid)
        {
            return Result<ConfirmTwoFactorSetupResponse>.Failure("Invalid verification code");
        }

        var encryptedSecret = _encryptionService.Encrypt(secretKey);
        user.EnableTwoFactorAuthentication(encryptedSecret);

        var recoveryCodes = _recoveryCodeGenerator.GenerateCodes();
        var recoveryCodeEntities = new List<RecoveryCode>();

        foreach (var code in recoveryCodes)
        {
            var hashedCode = _recoveryCodeGenerator.HashCode(code);
            var recoveryCode = RecoveryCode.Create(user.Id, hashedCode);
            recoveryCodeEntities.Add(recoveryCode);
            await _recoveryCodeRepository.AddAsync(recoveryCode, cancellationToken);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Security,
                Message = "Two-factor authentication enabled",
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

        var response = new ConfirmTwoFactorSetupResponse
        {
            RecoveryCodes = formattedCodes
        };

        return Result<ConfirmTwoFactorSetupResponse>.Success(response);
    }
}
