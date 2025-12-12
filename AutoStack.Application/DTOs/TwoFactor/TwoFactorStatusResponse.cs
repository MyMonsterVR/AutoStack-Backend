namespace AutoStack.Application.DTOs.TwoFactor;

/// <summary>
/// Response DTO containing the current two-factor authentication status for a user
/// </summary>
public class TwoFactorStatusResponse
{
    /// <summary>
    /// Whether 2FA is enabled for the user
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// When 2FA was enabled (null if not enabled)
    /// </summary>
    public DateTime? EnabledAt { get; set; }

    /// <summary>
    /// Number of unused recovery codes remaining
    /// </summary>
    public int RecoveryCodesRemaining { get; set; }
}
