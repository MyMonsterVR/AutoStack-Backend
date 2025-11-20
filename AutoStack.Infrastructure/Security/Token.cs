using AutoStack.Application.Common.Interfaces.Auth;

namespace AutoStack.Infrastructure.Security;

public class Token : IToken
{
    public string GenerateAccessToken(Guid userId, string username, string email)
    {
        return "TEST_TOKEN";
    }
    public string GenerateRefreshToken(Guid userId)
    {
        throw new NotImplementedException();
    }
    public bool VerifyToken(string token)
    {
        throw new NotImplementedException();
    }
}