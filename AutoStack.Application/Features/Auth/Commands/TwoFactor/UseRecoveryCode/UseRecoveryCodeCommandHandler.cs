using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.UseRecoveryCode;

public class UseRecoveryCodeCommandHandler : ICommandHandler<UseRecoveryCodeCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IToken _token;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecoveryCodeGenerator _recoveryCodeGenerator;
    private readonly IAuditLogService _auditLogService;

    public UseRecoveryCodeCommandHandler(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IToken token,
        IUnitOfWork unitOfWork,
        IRecoveryCodeGenerator recoveryCodeGenerator,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _token = token;
        _unitOfWork = unitOfWork;
        _recoveryCodeGenerator = recoveryCodeGenerator;
        _auditLogService = auditLogService;
    }

    public async Task<Result<LoginResponse>> Handle(UseRecoveryCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _token.VerifyTwoFactorToken(request.TwoFactorToken);
        if (userId == null)
        {
            return Result<LoginResponse>.Failure("Invalid or expired two-factor token");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("User not found");
        }

        if (!user.TwoFactorEnabled)
        {
            return Result<LoginResponse>.Failure("Two-factor authentication is not enabled");
        }

        // Get unused recovery codes for user
        var recoveryCodes = await _recoveryCodeRepository.GetUnusedByUserIdAsync(user.Id, cancellationToken);

        if (!recoveryCodes.Any())
        {
            return Result<LoginResponse>.Failure("No recovery codes available");
        }

        // Find matching recovery code
        var matchedCode = recoveryCodes.FirstOrDefault(code => _recoveryCodeGenerator.VerifyCode(request.RecoveryCode, code.CodeHash));

        if (matchedCode == null)
        {
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = "Failed recovery code verification attempt",
                    UserIdOverride = user.Id,
                    UsernameOverride = user.Username
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<LoginResponse>.Failure("Invalid recovery code");
        }

        matchedCode.MarkAsUsed();
        await _recoveryCodeRepository.UpdateAsync(matchedCode, cancellationToken);

        var accessToken = _token.GenerateAccessToken(user.Id, user.Username, user.Email);
        var refreshToken = _token.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log successful login with recovery code
        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Warning,
                Category = LogCategory.Security,
                Message = "User logged in with recovery code",
                UserIdOverride = user.Id,
                UsernameOverride = user.Username
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failures
        }

        var response = new LoginResponse
        {
            RequiresTwoFactor = false,
            TwoFactorToken = null,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };

        return Result<LoginResponse>.Success(response);
    }
}
