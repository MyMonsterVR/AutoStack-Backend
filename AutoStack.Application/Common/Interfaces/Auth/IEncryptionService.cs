namespace AutoStack.Application.Common.Interfaces.Auth;

public interface IEncryptionService
{
    string Encrypt(string text);
    string Decrypt(string encryptedText);
}