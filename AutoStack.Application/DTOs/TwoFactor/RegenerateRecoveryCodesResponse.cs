namespace AutoStack.Application.DTOs.TwoFactor;

/// <summary>
/// Response DTO containing newly generated recovery codes
/// </summary>
public class RegenerateRecoveryCodesResponse
{
    /// <summary>
    /// List of new recovery codes
    /// </summary>
    public required List<string> RecoveryCodes { get; set; }
}
