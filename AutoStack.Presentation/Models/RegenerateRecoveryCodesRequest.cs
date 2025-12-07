namespace AutoStack.Presentation.Models;

public record RegenerateRecoveryCodesRequest(
    string Password,
    string TotpCode
);
