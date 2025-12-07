using AutoStack.Application.Common.Interfaces;
using Resend;

namespace AutoStack.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IResend _resend;

    public EmailService(IResend resend)
    {
        _resend = resend;
    }
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new EmailMessage
        {
            From = "AutoStack <no-reply@autostack.dk>",
            To = email,
            Subject = subject,
            HtmlBody = message,
        };

        await _resend.EmailSendAsync(emailMessage);
    }
}