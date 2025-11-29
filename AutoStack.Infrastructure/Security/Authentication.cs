using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AutoStack.Infrastructure.Security;

public class Authentication : IAuthentication
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ApplicationDbContext _dbContext;
    private readonly Lazy<string> _dummyHash;

    public Authentication(IPasswordHasher passwordHasher, ApplicationDbContext dbContext)
    {
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _dummyHash = new Lazy<string>(() =>
            _passwordHasher.HashPassword("dummy_password_for_timing_attack_mitigation"));
    }

    /// <summary>
    /// Validates the user's inputted credentials
    /// </summary>
    /// <param name="username">The user inputted username</param>
    /// <param name="password">The user inputted password</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User id; null if credentials are invalid</returns>
    public async Task<Guid?> ValidateAuthenticationAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        // Use dummy hash if user doesn't exist, this prevents timing attacks
        var hashToVerify = user?.PasswordHash ?? _dummyHash.Value;
        var isMatched = _passwordHasher.VerifyPassword(password, hashToVerify);

        // Return null if user not found OR password doesn't match
        if (user == null || !isMatched)
        {
            return null;
        }

        return user.Id;
    }
}