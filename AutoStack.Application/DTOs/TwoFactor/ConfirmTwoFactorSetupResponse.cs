namespace AutoStack.Application.DTOs.TwoFactor;

/// <summary>
/// Response DTO for confirming two-factor authentication setup containing recovery codes
/// </summary>
public class ConfirmTwoFactorSetupResponse
{
    /// <summary>
    /// List of recovery codes
    /// </summary>
    public required List<string> RecoveryCodes { get; set; }
}
