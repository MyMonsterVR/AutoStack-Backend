using AutoStack.Application.Common.Interfaces.Auth;
using OtpNet;
using QRCoder;

namespace AutoStack.Infrastructure.Security;

public class TotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        var secretKeyBytes = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKeyBytes);
    }

    public bool ValidateCode(string secretKey, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));

        return totp.VerifyTotp(
            code,
            out _,
            new VerificationWindow(1, 1)
        );
    }

    /// <summary>
    /// Generates an OTP authentication link
    /// </summary>
    /// <param name="secretKey">The secret key to be used for OTP</param>
    /// <param name="email">The email of the user</param>
    /// <param name="issuer">Our company name</param>
    /// <returns>a link matching otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}</returns>
    public string GenerateTotpUri(string secretKey, string email, string issuer)
    {
        return new OtpUri(OtpType.Totp, secretKey, email, issuer).ToString();
    }

    /// <summary>
    /// Generates a QR Code that can be used for OTP authentication app connection
    /// </summary>
    /// <param name="totpUri">The uri of the TOTP</param>
    /// <returns></returns>
    public byte[] GenerateQrCodeBase64(string totpUri)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
        using var code = new PngByteQRCode(data);

        return code.GetGraphic(20);
    }
}