using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handles the login command by validating credentials and generating _authentication tokens
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthentication _authentication;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IToken _token;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public LoginCommandHandler(
        IAuthentication authentication,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IToken token,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService)
    {
        _authentication = authentication;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _token = token;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Processes the login request by validating credentials and returning access and refresh tokens
    /// </summary>
    /// <param name="request">The login command containing username and password</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the login response with tokens on success, or an error message on failure</returns>
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _authentication.ValidateAuthenticationAsync(request.Username.ToLower(), request.Password, cancellationToken);

        if (userId == null || userId.Value == Guid.Empty)
        {
            // Log failed login attempt (SECURITY EVENT)
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = $"Failed login attempt for username: {request.Username}",
                    UsernameOverride = request.Username
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<LoginResponse>.Failure("Invalid username or password");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("User not found");
        }

        // Check if 2FA is enabled
        if (user.TwoFactorEnabled)
        {
            var twoFactorToken = _token.GenerateTwoFactorToken(userId.Value);

            var twoFactorResponse = new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorToken = twoFactorToken,
                AccessToken = null,
                RefreshToken = null
            };

            return Result<LoginResponse>.Success(twoFactorResponse);
        }

        // Normal login flow (no 2FA)
        var generatedToken = _token.GenerateAccessToken(userId.Value, user.Username, user.Email);
        var userToken = _token.GenerateRefreshToken(userId.Value);

        await _refreshTokenRepository.AddAsync(userToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log successful login (SECURITY EVENT)
        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Authentication,
                Message = "User logged in successfully",
                UserIdOverride = userId.Value,
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
            AccessToken = generatedToken,
            RefreshToken = userToken.Token
        };

        return Result<LoginResponse>.Success(response);
    }
}