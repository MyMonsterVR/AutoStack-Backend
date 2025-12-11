using System.Security.Cryptography;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Login;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace AutoStack.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handles the registration command by creating a new user account
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLogService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Processes the registration request by validating the data and creating a new user
    /// </summary>
    /// <param name="request">The registration command containing user details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the registered user's ID or failure with an error message</returns>
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExists(request.Email.ToLower(), cancellationToken))
        {
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

            return Result<RegisterResponse>.Failure("Email already exists");
        }

        if (await _userRepository.UsernameExists(request.Username.ToLower(), cancellationToken))
        {
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

            return Result<RegisterResponse>.Failure("Username already exists");
        }

        if (request.Password != request.ConfirmPassword)
        {
            return Result<RegisterResponse>.Failure("Passwords do not match");
        }

        var user = User.CreateUser(request.Email.ToLower(), request.Username.ToLower());

        var passwordHashed = _passwordHasher.HashPassword(request.Password);
        user.SetPassword(passwordHashed);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate and send verification email
        var verificationCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiryMinutes = _configuration.GetValue("EmailVerification:ExpiryMinutes", 15);

        user.SetEmailVerificationCode(verificationCode, DateTime.UtcNow.AddMinutes(expiryMinutes));
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendVerificationEmail(user.Email, user.Username, verificationCode, expiryMinutes);

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

        return Result<RegisterResponse>.Success(new RegisterResponse(user.Id));
    }

    private async Task SendVerificationEmail(string email, string username, string code, int expiryMinutes)
    {
        await _emailService.SendEmailAsync(
            email,
            "Verify Your Email - AutoStack",
            GenerateEmailHtml(username, code, expiryMinutes)
        );
    }

    private static string GenerateEmailHtml(string username, string code, int expiryMinutes)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f5;">
            <table role="presentation" style="width: 100%; border-collapse: collapse;">
                <tr>
                    <td align="center" style="padding: 40px 0;">
                        <table role="presentation" style="width: 600px; max-width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                            <tr>
                                <td style="padding: 40px 40px 30px 40px; text-align: center; border-bottom: 1px solid #e4e4e7;">
                                    <h1 style="margin: 0; color: #18181b; font-size: 28px; font-weight: 700;">Verify Your Email</h1>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 40px;">
                                    <p style="margin: 0 0 20px 0; color: #52525b; font-size: 16px; line-height: 24px;">
                                        Hi <strong>{username}</strong>,
                                    </p>
                                    <p style="margin: 0 0 20px 0; color: #52525b; font-size: 16px; line-height: 24px;">
                                        Thank you for creating an account with AutoStack! Please use the following verification code to verify your email address:
                                    </p>

                                    <table role="presentation" style="margin: 30px 0; width: 100%;">
                                        <tr>
                                            <td align="center">
                                                <div style="display: inline-block; padding: 16px 32px; background-color: #f4f4f5; border-radius: 8px; font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #18181b;">
                                                    {code}
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <p style="margin: 20px 0 0 0; color: #71717a; font-size: 14px; line-height: 20px;">
                                        This code will expire in <strong>{expiryMinutes} minutes</strong>.
                                    </p>

                                    <p style="margin: 20px 0 0 0; color: #71717a; font-size: 14px; line-height: 20px;">
                                        If you didn't create an account with AutoStack, you can safely ignore this email.
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 30px 40px; background-color: #fafafa; border-top: 1px solid #e4e4e7; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;">
                                    <p style="margin: 0; color: #a1a1aa; font-size: 12px; line-height: 18px; text-align: center;">
                                        This email was sent by AutoStack. If you have questions, please contact support.
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
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