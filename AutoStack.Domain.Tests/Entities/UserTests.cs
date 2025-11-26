using AutoStack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AutoStack.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidParameters_ShouldCreateUser()
    {
        var email = "test@example.com";
        var username = "testuser";

        var user = User.CreateUser(email, username);

        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Username.Should().Be(username);
        user.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateUser_WithEmptyEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        var username = "testuser";

        Action act = () => User.CreateUser(invalidEmail!, username);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty*")
            .WithParameterName("email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateUser_WithEmptyUsername_ShouldThrowArgumentException(string? invalidUsername)
    {
        var email = "test@example.com";

        Action act = () => User.CreateUser(email, invalidUsername!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Username cannot be null or empty*")
            .WithParameterName("username");
    }

    [Fact]
    public void CreateUser_ShouldGenerateNewGuid()
    {
        var user1 = User.CreateUser("test1@example.com", "user1");
        var user2 = User.CreateUser("test2@example.com", "user2");

        user1.Id.Should().NotBeEmpty();
        user2.Id.Should().NotBeEmpty();
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void CreateUser_ShouldSetTimestamps()
    {
        var before = DateTime.UtcNow;

        var user = User.CreateUser("test@example.com", "testuser");

        var after = DateTime.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        user.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void SetPassword_WithValidHash_ShouldSetPasswordHash()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        var passwordHash = "hashed_password_123";

        user.SetPassword(passwordHash);

        user.PasswordHash.Should().Be(passwordHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void SetPassword_WithEmptyHash_ShouldThrowArgumentException(string? invalidHash)
    {
        var user = User.CreateUser("test@example.com", "testuser");

        Action act = () => user.SetPassword(invalidHash!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Password hash cannot be empty*")
            .WithParameterName("passwordHash");
    }

    [Fact]
    public void SetPassword_ShouldUpdateTimestamp()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(10);

        user.SetPassword("hashed_password_123");

        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
