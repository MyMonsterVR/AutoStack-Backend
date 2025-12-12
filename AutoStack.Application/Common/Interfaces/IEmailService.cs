namespace AutoStack.Application.Common.Interfaces;

/// <summary>
/// Service for sending email messages
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message to the specified recipient
    /// </summary>
    /// <param name="email">The recipient email address</param>
    /// <param name="subject">The email subject</param>
    /// <param name="message">The email message body</param>
    Task SendEmailAsync(string email, string subject, string message);
}