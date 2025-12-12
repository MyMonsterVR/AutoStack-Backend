namespace AutoStack.Presentation.Models;

/// <summary>
/// Request model for regenerating two-factor authentication recovery codes
/// </summary>
/// <param name="Password">The user's current password for verification</param>
/// <param name="TotpCode">The current TOTP code for verification</param>
public record RegenerateRecoveryCodesRequest(
    string Password,
    string TotpCode
);
