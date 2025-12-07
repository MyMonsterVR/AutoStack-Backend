using System.Security.Cryptography;
using System.Text;
using AutoStack.Application.Common.Interfaces.Auth;

namespace AutoStack.Infrastructure.Security;

/// <summary>
/// Generates and manages recovery codes for 2FA backup access
/// </summary>
public class RecoveryCodeGenerator : IRecoveryCodeGenerator
{
    private const string ValidChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // Excludes 0,1,O,I for clarity
    private const int CodeLength = 10;
    private const int CodeCount = 10;

    /// <summary>
    /// Generates a list of random recovery codes
    /// </summary>
    /// <returns>List of plain-text recovery codes</returns>
    public List<string> GenerateCodes()
    {
        var codes = new List<string>(CodeCount);

        for (int i = 0; i < CodeCount; i++)
        {
            var codeChars = new char[CodeLength];
            var randomBytes = new byte[CodeLength];
            RandomNumberGenerator.Fill(randomBytes);

            for (int j = 0; j < CodeLength; j++)
            {
                codeChars[j] = ValidChars[randomBytes[j] % ValidChars.Length];
            }

            codes.Add(new string(codeChars));
        }

        return codes;
    }

    /// <summary>
    /// Hashes a recovery code for secure storage
    /// </summary>
    /// <param name="code">The plain-text recovery code</param>
    /// <returns>Base64-encoded SHA-256 hash of the code</returns>
    public string HashCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        // Normalize: remove hyphens and convert to uppercase
        var normalizedCode = code.Replace("-", "").ToUpperInvariant();

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedCode));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Formats a recovery code for display (XXXX-XXXX-XX)
    /// </summary>
    /// <param name="code">The plain-text recovery code (10 characters)</param>
    /// <returns>Formatted code with hyphens for readability</returns>
    public string FormatForDisplay(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
            throw new ArgumentException($"Code must be exactly {CodeLength} characters", nameof(code));

        return $"{code[..4]}-{code[4..8]}-{code[8..]}";
    }

    /// <summary>
    /// Verifies a recovery code against a hash using constant-time comparison
    /// </summary>
    /// <param name="code">The plain-text code to verify</param>
    /// <param name="hash">The stored hash to compare against</param>
    /// <returns>True if the code matches the hash</returns>
    public bool VerifyCode(string code, string hash)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var computedHash = HashCode(code);
            var storedHashBytes = Convert.FromBase64String(hash);
            var computedHashBytes = Convert.FromBase64String(computedHash);

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
        }
        catch
        {
            return false;
        }
    }
}