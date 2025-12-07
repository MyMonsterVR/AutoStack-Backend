namespace AutoStack.Application.Common.Interfaces.Auth;

public interface ITotpService
{
    string GenerateSecretKey();

    bool ValidateCode(string secretKey, string code);

    string GenerateTotpUri(string secretKey, string email, string issuer);

    byte[] GenerateQrCodeBase64(string totpUri);
}