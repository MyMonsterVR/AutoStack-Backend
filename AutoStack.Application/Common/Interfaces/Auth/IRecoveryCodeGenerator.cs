namespace AutoStack.Application.Common.Interfaces.Auth;

public interface IRecoveryCodeGenerator
{
    /// <summary>
    /// Generates a list of random recovery codes
    /// </summary>
    List<string> GenerateCodes();

    /// <summary>
    /// Hashes a recovery code for secure storage
    /// </summary>
    string HashCode(string code);

    /// <summary>
    /// Formats a recovery code for display (XXXX-XXXX-XX)
    /// </summary>
    string FormatForDisplay(string code);

    /// <summary>
    /// Verifies a recovery code against a hash
    /// </summary>
    bool VerifyCode(string code, string hash);
}
