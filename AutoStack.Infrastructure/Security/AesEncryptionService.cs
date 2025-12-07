using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Infrastructure.Security.Models;
using Microsoft.Extensions.Options;

namespace AutoStack.Infrastructure.Security;

public class AesEncryptionService : IEncryptionService
{
    private readonly TwoFactorSettings _settings;

    public AesEncryptionService(IOptions<TwoFactorSettings> settings)
    {
        _settings = settings.Value;
    }
    
    public string Encrypt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Plain text cannot be null or empty", nameof(text));
        }

        var key = Convert.FromBase64String(_settings.EncryptionKey);

        var plainBytes = Encoding.UTF8.GetBytes(text);

        // Generate random nonce (12 bytes for AES-GCM)
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        // Prepare buffers
        var cipherText = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        // Encrypt
        using var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);

        // Combines nonce + tag + ciphertext
        var result = new byte[nonce.Length + tag.Length + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherText, 0, result, nonce.Length + tag.Length, cipherText.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            throw new ArgumentException($"Encrypted text cannot be null or whitespace", nameof(encryptedText));
        }

        var key = Convert.FromBase64String(_settings.EncryptionKey);

        var combinedBytes = Convert.FromBase64String(encryptedText);

        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;

        var nonce = new byte[nonceSize];
        var tag = new byte[tagSize];
        var encryptedData = new byte[combinedBytes.Length - nonceSize - tagSize];
        
        Buffer.BlockCopy(combinedBytes, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(combinedBytes, nonceSize, tag, 0, tagSize);
        Buffer.BlockCopy(combinedBytes, nonceSize + tagSize, encryptedData, 0, encryptedData.Length);
        
        var plainBytes = new byte[encryptedData.Length];

        using var aesGcm = new AesGcm(key, tagSize);
        aesGcm.Decrypt(nonce, encryptedData, tag, plainBytes);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
}