namespace AutoStack.Infrastructure.Security.Models;

/// <summary>
/// Configuration settings for two-factor authentication
/// </summary>
public class TwoFactorSettings
{
    /// <summary>
    /// Gets or sets the encryption key used to encrypt TOTP secret keys
    /// </summary>
    public string EncryptionKey { get; set; }
}