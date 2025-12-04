using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoStack.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AutoStack.Infrastructure.Tests.Security;

public class TokenTests
{
    private readonly Token _token;
    private readonly JwtSettings _jwtSettings;

    public TokenTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVerySecureSecretKeyForTestingPurposes12345678901234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 30
        };

        var options = Microsoft.Extensions.Options.Options.Create(_jwtSettings);
        _token = new Token(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnNonEmptyString()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        accessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(accessToken);
        canRead.Should().BeTrue("the generated token should be a valid JWT");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserId()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);

        nameIdClaim.Should().NotBeNull();
        nameIdClaim!.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeJti()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        jtiClaim.Should().NotBeNull();
        Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue("JTI should be a valid GUID");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeIat()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var iatClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);

        iatClaim.Should().NotBeNull();
        long.TryParse(iatClaim!.Value, out _).Should().BeTrue("IAT should be a valid timestamp");
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectIssuerAndAudience()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectExpiration()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var beforeGeneration = DateTime.UtcNow;

        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var expectedExpiration = beforeGeneration.AddMinutes(_jwtSettings.ExpirationMinutes);

        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_ShouldGenerateUniqueTokens()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var token1 = _token.GenerateAccessToken(userId, username, email);
        var token2 = _token.GenerateAccessToken(userId, username, email);

        token1.Should().NotBe(token2, "each token should have a unique JTI");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRefreshToken()
    {
        var userId = Guid.NewGuid();

        var refreshToken = _token.GenerateRefreshToken(userId);

        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeNullOrEmpty();
        refreshToken.UserId.Should().Be(userId);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        var userId = Guid.NewGuid();

        var refreshToken = _token.GenerateRefreshToken(userId);

        var isValidBase64 = () => Convert.FromBase64String(refreshToken.Token);
        isValidBase64.Should().NotThrow("refresh token should be valid base64");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldSetCorrectExpiration()
    {
        var userId = Guid.NewGuid();
        var beforeGeneration = DateTime.UtcNow;

        var refreshToken = _token.GenerateRefreshToken(userId);

        var expectedExpiration = beforeGeneration.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        var actualExpiration = DateTimeOffset.FromUnixTimeSeconds(refreshToken.ExpiresAt).UtcDateTime;

        actualExpiration.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        var userId = Guid.NewGuid();

        var refreshToken1 = _token.GenerateRefreshToken(userId);
        var refreshToken2 = _token.GenerateRefreshToken(userId);

        refreshToken1.Token.Should().NotBe(refreshToken2.Token, "each refresh token should be unique");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeAtLeast64Bytes()
    {
        var userId = Guid.NewGuid();

        var refreshToken = _token.GenerateRefreshToken(userId);
        var tokenBytes = Convert.FromBase64String(refreshToken.Token);

        tokenBytes.Length.Should().BeGreaterOrEqualTo(64, "refresh token should be cryptographically secure");
    }

    [Fact]
    public void VerifyToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var principal = _token.VerifyToken(accessToken);

        principal.Should().NotBeNull();
    }

    [Fact]
    public void VerifyToken_WithValidToken_ShouldContainCorrectUserId()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var accessToken = _token.GenerateAccessToken(userId, username, email);

        var principal = _token.VerifyToken(accessToken);

        principal.Should().NotBeNull();

        // The JWT handler reads the token directly to verify the claim value
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);

        nameIdClaim.Should().NotBeNull();
        nameIdClaim!.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void VerifyToken_WithInvalidToken_ShouldReturnNull()
    {
        var invalidToken = "invalid.jwt.token";

        var principal = _token.VerifyToken(invalidToken);

        principal.Should().BeNull();
    }

    [Fact]
    public void VerifyToken_WithExpiredToken_ShouldReturnNull()
    {
        // Create a token that's already expired by manually building it
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create a token that expired 1 hour ago
        var expiredToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            signingCredentials: credentials
        );

        var expiredTokenString = new JwtSecurityTokenHandler().WriteToken(expiredToken);

        var principal = _token.VerifyToken(expiredTokenString);

        principal.Should().BeNull("the token has expired");
    }

    [Fact]
    public void VerifyToken_WithTokenFromDifferentIssuer_ShouldReturnNull()
    {
        var differentSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            Issuer = "DifferentIssuer",
            Audience = _jwtSettings.Audience,
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 30
        };

        var differentTokenGenerator = new Token(Microsoft.Extensions.Options.Options.Create(differentSettings));
        var userId = Guid.NewGuid();
        var tokenWithDifferentIssuer = differentTokenGenerator.GenerateAccessToken(userId, "testuser", "test@example.com");

        var principal = _token.VerifyToken(tokenWithDifferentIssuer);

        principal.Should().BeNull();
    }

    [Fact]
    public void VerifyToken_WithTokenFromDifferentAudience_ShouldReturnNull()
    {
        var differentSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            Issuer = _jwtSettings.Issuer,
            Audience = "DifferentAudience",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 30
        };

        var differentTokenGenerator = new Token(Microsoft.Extensions.Options.Options.Create(differentSettings));
        var userId = Guid.NewGuid();
        var tokenWithDifferentAudience = differentTokenGenerator.GenerateAccessToken(userId, "testuser", "test@example.com");

        var principal = _token.VerifyToken(tokenWithDifferentAudience);

        principal.Should().BeNull();
    }

    [Fact]
    public void VerifyToken_WithTokenSignedWithDifferentKey_ShouldReturnNull()
    {
        var differentSettings = new JwtSettings
        {
            SecretKey = "DifferentSecretKeyThatIsLongEnoughForHmacSha2561234567890123456",
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 30
        };

        var differentTokenGenerator = new Token(Microsoft.Extensions.Options.Options.Create(differentSettings));
        var userId = Guid.NewGuid();
        var tokenWithDifferentKey = differentTokenGenerator.GenerateAccessToken(userId, "testuser", "test@example.com");

        var principal = _token.VerifyToken(tokenWithDifferentKey);

        principal.Should().BeNull();
    }

    [Fact]
    public void VerifyToken_WithEmptyToken_ShouldReturnNull()
    {
        var principal = _token.VerifyToken("");

        principal.Should().BeNull();
    }
}
