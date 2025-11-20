namespace AutoStack.Application.Common.Interfaces.Auth;

public interface IToken
{
    /// <summary>
    /// Generates an access token
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <param name="username">The users username</param>
    /// <param name="email">The users email</param>
    /// <returns></returns>
    string GenerateAccessToken(Guid userId, string username, string email);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <returns></returns>
    string GenerateRefreshToken(Guid userId);
    
    /// <summary>
    /// Verifies a token is valid
    /// </summary>
    /// <param name="token">The users auth token</param>
    /// <returns></returns>
    bool VerifyToken(string token);
}