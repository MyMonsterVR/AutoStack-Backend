using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AutoStack.Infrastructure.Security;

public class Authentication(IPasswordHasher passwordHasher, ApplicationDbContext dbContext) : IAuthentication
{
    public async Task<Guid?> ValidateAuthenticationAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        var isMatched = passwordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isMatched)
        {
            return null;
        }

        return user.Id;
    }
}