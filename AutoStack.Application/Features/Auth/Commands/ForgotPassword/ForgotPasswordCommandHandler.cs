using System.Configuration;
using System.Security.Cryptography;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace AutoStack.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        IEmailService emailService,
        IConfiguration configuration
    )
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _emailService = emailService;
        _configuration = configuration;
    }
    
    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Failure("User not found");
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.SetPasswordResetToken(token, DateTime.UtcNow.AddMinutes(15));

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["FrontendUrl"];

        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            throw new ConfigurationErrorsException("FrontendUrl is missing");
        }
        
        var resetUrl = $"{frontendUrl}/resetpassword?token={token}";

        await _emailService.SendEmailAsync(
            user.Email,
            "Reset Your Password - AutoStack",
            $"""
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
                                        <h1 style="margin: 0; color: #18181b; font-size: 28px; font-weight: 700;">Reset Your Password</h1>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 40px;">
                                        <p style="margin: 0 0 20px 0; color: #52525b; font-size: 16px; line-height: 24px;">
                                            Hi <strong>{user.Username}</strong>,
                                        </p>
                                        <p style="margin: 0 0 20px 0; color: #52525b; font-size: 16px; line-height: 24px;">
                                            We received a request to reset your password for your AutoStack account. Click the button below to choose a new password.
                                        </p>

                                        <table role="presentation" style="margin: 30px 0; width: 100%;">
                                            <tr>
                                                <td align="center">
                                                    <a href="{resetUrl}" style="display: inline-block; padding: 14px 32px; background-color: #3b82f6; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 16px; border-radius: 6px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
                                                        Reset Password
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style="margin: 20px 0 0 0; color: #71717a; font-size: 14px; line-height: 20px;">
                                            This link will expire in <strong>15 minutes</strong> for security reasons.
                                        </p>

                                        <p style="margin: 20px 0 0 0; color: #71717a; font-size: 14px; line-height: 20px;">
                                            If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
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
            """
        );
        
        try
        {
            await _auditLogService.LogAsync(new AuditLogRequest
            {
                Level = LogLevel.Information,
                Category = LogCategory.Security,
                Message = "Requested forgot password reset link",
                UserIdOverride = user.Id,
                UsernameOverride = user.Username
            }, cancellationToken);
        }
        catch
        {
            // Ignore logging failure
        }
        
        return Result<bool>.Success(true);
    }
}