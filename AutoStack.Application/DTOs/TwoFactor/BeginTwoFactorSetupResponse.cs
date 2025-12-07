namespace AutoStack.Application.DTOs.TwoFactor;

public class BeginTwoFactorSetupResponse
{
    /// <summary>
    /// The setup token (short-lived JWT containing the secret)
    /// </summary>
    public required string SetupToken { get; set; }

    /// <summary>
    /// The secret key in Base32 format for manual entry
    /// </summary>
    public required string ManualEntryKey { get; set; }

    /// <summary>
    /// QR code image as PNG bytes
    /// </summary>
    public required byte[] QrCode { get; set; }
}
