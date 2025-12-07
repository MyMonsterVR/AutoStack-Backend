namespace AutoStack.Presentation.Models;

public record DisableTwoFactorRequest(
    string Password,
    string TotpCode
);
