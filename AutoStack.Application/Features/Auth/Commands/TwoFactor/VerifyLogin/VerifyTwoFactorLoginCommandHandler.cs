using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.VerifyLogin;

public class VerifyTwoFactorLoginCommandHandler : ICommandHandler<VerifyTwoFactorLoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly IToken _token;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public VerifyTwoFactorLoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITotpService totpService,
        IEncryptionService encryptionService,
        IToken token,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _token = token;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<Result<LoginResponse>> Handle(VerifyTwoFactorLoginCommand request, CancellationToken cancellationToken)
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

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            return Result<LoginResponse>.Failure("Two-factor authentication is not enabled");
        }

        // Decrypt secret and validate TOTP code
        var secretKey = _encryptionService.Decrypt(user.TwoFactorSecretKey);
        var codeValid = _totpService.ValidateCode(secretKey, request.TotpCode);

        if (!codeValid)
        {
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = "Failed 2FA verification attempt",
                    UserIdOverride = user.Id,
                    UsernameOverride = user.Username
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<LoginResponse>.Failure("Invalid verification code");
        }

        var accessToken = _token.GenerateAccessToken(user.Id, user.Username, user.Email);
        var refreshToken = _token.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Authentication,
                Message = "User logged in successfully with 2FA",
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
