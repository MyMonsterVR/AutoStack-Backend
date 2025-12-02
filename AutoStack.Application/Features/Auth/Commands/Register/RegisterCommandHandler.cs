using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles the registration command by creating a new user account
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Processes the registration request by validating the data and creating a new user
    /// </summary>
    /// <param name="request">The registration command containing user details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result indicating success or failure with an error message</returns>
    public async Task<Result<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExists(request.Email.ToLower(), cancellationToken))
        {
            // Log failed registration attempt - email exists
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = "Registration attempt failed - email already exists",
                    UsernameOverride = request.Username,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["Email"] = MaskEmail(request.Email),
                        ["Username"] = request.Username,
                        ["Reason"] = "EmailExists"
                    }
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<bool>.Failure("Email already exists");
        }

        if (await _userRepository.UsernameExists(request.Username.ToLower(), cancellationToken))
        {
            // Log failed registration attempt - username exists
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Security,
                    Message = "Registration attempt failed - username already exists",
                    UsernameOverride = request.Username,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["Email"] = MaskEmail(request.Email),
                        ["Username"] = request.Username,
                        ["Reason"] = "UsernameExists"
                    }
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Result<bool>.Failure("Username already exists");
        }

        if (request.Password != request.ConfirmPassword)
        {
            return Result<bool>.Failure("Passwords do not match");
        }

        var user = User.CreateUser(request.Email.ToLower(), request.Username.ToLower());

        var passwordHashed = _passwordHasher.HashPassword(request.Password);
        user.SetPassword(passwordHashed);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log successful registration
        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Authentication,
                Message = "User registered successfully",
                UserIdOverride = user.Id,
                UsernameOverride = user.Username,
                AdditionalData = new Dictionary<string, object>
                {
                    ["UserId"] = user.Id,
                    ["Username"] = user.Username,
                    ["Email"] = MaskEmail(user.Email)
                }
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failures
        }

        return Result<bool>.Success(true);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "null";

        var parts = email.Split('@');
        if (parts.Length != 2) return "invalid";

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"{localPart[0]}***@{domain}";

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }
}