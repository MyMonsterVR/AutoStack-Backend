using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AutoStack.Infrastructure.Security;

public class Authentication(IPasswordHasher passwordHasher, ApplicationDbContext dbContext) : IAuthentication
{
    private readonly Lazy<string> _dummyHash = new(() =>
        passwordHasher.HashPassword("dummy_password_for_timing_attack_mitigation"));

    public async Task<Guid?> ValidateAuthenticationAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        // Use dummy hash if user doesn't exist, this prevents timing attacks
        var hashToVerify = user?.PasswordHash ?? _dummyHash.Value;
        var isMatched = passwordHasher.VerifyPassword(password, hashToVerify);

        // Return null if user not found OR password doesn't match
        if (user == null || !isMatched)
        {
            return null;
        }

        return user.Id;
    }
}