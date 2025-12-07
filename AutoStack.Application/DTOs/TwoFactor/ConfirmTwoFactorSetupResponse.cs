namespace AutoStack.Application.DTOs.TwoFactor;

public class ConfirmTwoFactorSetupResponse
{
    /// <summary>
    /// List of recovery codes
    /// </summary>
    public required List<string> RecoveryCodes { get; set; }
}
