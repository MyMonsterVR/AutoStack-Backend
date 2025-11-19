using System.Security.Cryptography;
using System.Text;
using AutoStack.Application.Common.Interfaces;
using Konscious.Security.Cryptography;

namespace AutoStack.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DegreeOfParallelism = 8;
    private const int Iterations = 4;
    private const int MemorySize = 1024 * 1024; 
    
    /// <summary>
    /// Hashes a password
    /// </summary>
    /// <param name="password">User inputted password</param>
    /// <returns>Hashed password</returns>
    public string HashPassword(string password)
    {
        // Generate a random salt
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = HashPasswordInternally(password, salt);

        // Combine salt and hash
        var combinedBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, combinedBytes, 0, salt.Length);
        Array.Copy(hash, 0, combinedBytes, salt.Length, hash.Length);

        return Convert.ToBase64String(combinedBytes);
    }

    /// <summary>
    /// Hashes password
    /// </summary>
    /// <param name="password">User inputted password</param>
    /// <param name="salt">Randomly generated salt</param>
    /// <returns>Hashed password as bytes</returns>
    private static byte[] HashPasswordInternally(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations,
        };
        
        return argon2.GetBytes(HashSize);
    }

    /// <summary>
    /// Verifies user inputted password against known password
    /// </summary>
    /// <param name="password">User inputted password</param>
    /// <param name="hashedPassword">Known hashed password</param>
    /// <returns></returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var combinedBytes = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];
        
        Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
        Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);

        var newHash = HashPasswordInternally(password, salt);

        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }
}