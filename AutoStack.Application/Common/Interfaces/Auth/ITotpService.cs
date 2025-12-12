namespace AutoStack.Application.Common.Interfaces.Auth;

/// <summary>
/// Service for Time-based One-Time Password (TOTP) operations for two-factor authentication
/// </summary>
public interface ITotpService
{
    /// <summary>
    /// Generates a new secret key for TOTP setup
    /// </summary>
    /// <returns>A base32-encoded secret key</returns>
    string GenerateSecretKey();

    /// <summary>
    /// Validates a TOTP code against a secret key
    /// </summary>
    /// <param name="secretKey">The user's secret key</param>
    /// <param name="code">The TOTP code to validate</param>
    /// <returns>True if the code is valid, false otherwise</returns>
    bool ValidateCode(string secretKey, string code);

    /// <summary>
    /// Generates a TOTP URI for QR code generation
    /// </summary>
    /// <param name="secretKey">The user's secret key</param>
    /// <param name="email">The user's email address</param>
    /// <param name="issuer">The application/issuer name</param>
    /// <returns>A TOTP URI string</returns>
    string GenerateTotpUri(string secretKey, string email, string issuer);

    /// <summary>
    /// Generates a QR code image as base64-encoded bytes from a TOTP URI
    /// </summary>
    /// <param name="totpUri">The TOTP URI to encode</param>
    /// <returns>Base64-encoded QR code image bytes</returns>
    byte[] GenerateQrCodeBase64(string totpUri);
}