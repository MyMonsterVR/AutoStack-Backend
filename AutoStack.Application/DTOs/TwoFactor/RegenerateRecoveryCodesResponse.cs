namespace AutoStack.Application.DTOs.TwoFactor;

public class RegenerateRecoveryCodesResponse
{
    /// <summary>
    /// List of new recovery codes
    /// </summary>
    public required List<string> RecoveryCodes { get; set; }
}
