namespace AutoStack.Application.Common.Interfaces.Auth;

public interface IAuthentication
{
    /// <summary>
    /// Validates that the user is authenticated
    /// </summary>
    /// <param name="username">Request username</param>
    /// <param name="password">Request password</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The users guid if valid else null</returns>
    Task<Guid?> ValidateAuthenticationAsync(string username, string password, CancellationToken cancellationToken);
}