namespace AutoStack.Application.Common.Interfaces;

public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the password
    /// </summary>
    /// <param name="password">Password to be hashed</param>
    /// <returns></returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies that a hash password matches user given password
    /// </summary>
    /// <param name="password">Password from the user</param>
    /// <param name="hashedPassword">Password to be matched with users inputted password</param>
    /// <returns></returns>
    bool VerifyPassword(string password, string hashedPassword);
}