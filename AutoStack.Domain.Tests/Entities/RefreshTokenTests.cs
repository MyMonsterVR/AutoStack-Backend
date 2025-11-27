using AutoStack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AutoStack.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateRefreshToken()
    {
        var token = "test_token_12345";
        var userId = Guid.NewGuid();
        var expiresAt = GetFutureUnixTime(30);

        var refreshToken = RefreshToken.Create(token, userId, expiresAt);

        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().Be(token);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyToken_ShouldThrowArgumentException(string? invalidToken)
    {
        var userId = Guid.NewGuid();
        var expiresAt = GetFutureUnixTime(30);

        Action act = () => RefreshToken.Create(invalidToken!, userId, expiresAt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Token cannot be null or empty*")
            .WithParameterName("token");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var token = "test_token_12345";
        var expiresAt = GetFutureUnixTime(30);

        Action act = () => RefreshToken.Create(token, Guid.Empty, expiresAt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*")
            .WithParameterName("userId");
    }

    [Fact]
    public void Create_WithPastExpirationTime_ShouldThrowArgumentException()
    {
        var token = "test_token_12345";
        var userId = Guid.NewGuid();
        var pastExpiresAt = GetPastUnixTime(10);

        Action act = () => RefreshToken.Create(token, userId, pastExpiresAt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiration time must be in the future*")
            .WithParameterName("expiresAt");
    }

    [Fact]
    public void Create_WithCurrentExpirationTime_ShouldThrowArgumentException()
    {
        var token = "test_token_12345";
        var userId = Guid.NewGuid();
        var currentUnixTime = GetCurrentUnixTime();

        Action act = () => RefreshToken.Create(token, userId, currentUnixTime);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiration time must be in the future*")
            .WithParameterName("expiresAt");
    }

    [Fact]
    public void Create_ShouldGenerateNewGuid()
    {
        var token1 = "token1";
        var token2 = "token2";
        var userId = Guid.NewGuid();
        var expiresAt = GetFutureUnixTime(30);

        var refreshToken1 = RefreshToken.Create(token1, userId, expiresAt);
        var refreshToken2 = RefreshToken.Create(token2, userId, expiresAt);

        refreshToken1.Id.Should().NotBeEmpty();
        refreshToken2.Id.Should().NotBeEmpty();
        refreshToken1.Id.Should().NotBe(refreshToken2.Id);
    }

    [Fact]
    public void Create_ShouldSetTimestamps()
    {
        var token = "test_token_12345";
        var userId = Guid.NewGuid();
        var expiresAt = GetFutureUnixTime(30);
        var before = DateTime.UtcNow;

        var refreshToken = RefreshToken.Create(token, userId, expiresAt);

        var after = DateTime.UtcNow;
        refreshToken.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        refreshToken.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithLongToken_ShouldAcceptToken()
    {
        var longToken = new string('a', 128);
        var userId = Guid.NewGuid();
        var expiresAt = GetFutureUnixTime(30);

        var refreshToken = RefreshToken.Create(longToken, userId, expiresAt);

        refreshToken.Token.Should().Be(longToken);
        refreshToken.Token.Should().HaveLength(128);
    }

    [Fact]
    public void Create_WithFarFutureExpiration_ShouldAcceptExpiration()
    {
        var token = "test_token";
        var userId = Guid.NewGuid();
        var farFutureExpiresAt = GetFutureUnixTime(365); // 1 year

        var refreshToken = RefreshToken.Create(token, userId, farFutureExpiresAt);

        refreshToken.ExpiresAt.Should().Be(farFutureExpiresAt);
    }

    // Helper methods
    private static int GetCurrentUnixTime()
    {
        return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private static int GetFutureUnixTime(int daysInFuture)
    {
        return (int)DateTime.UtcNow.AddDays(daysInFuture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private static int GetPastUnixTime(int daysInPast)
    {
        return (int)DateTime.UtcNow.AddDays(-daysInPast).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
