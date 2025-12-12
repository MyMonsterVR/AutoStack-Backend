namespace AutoStack.Presentation.Models;

/// <summary>
/// Request model for disabling two-factor authentication
/// </summary>
/// <param name="Password">The user's current password for verification</param>
/// <param name="TotpCode">The current TOTP code for verification</param>
public record DisableTwoFactorRequest(
    string Password,
    string TotpCode
);
