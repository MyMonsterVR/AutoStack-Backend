namespace AutoStack.Application.Common.Interfaces.Auth;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plain text into an encrypted string
    /// </summary>
    /// <param name="text">The plain text to encrypt</param>
    /// <returns>The encrypted text</returns>
    string Encrypt(string text);

    /// <summary>
    /// Decrypts an encrypted string back to plain text
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt</param>
    /// <returns>The decrypted plain text</returns>
    string Decrypt(string encryptedText);
}