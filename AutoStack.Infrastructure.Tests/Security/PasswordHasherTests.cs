using AutoStack.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace AutoStack.Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyString()
    {
        var password = "testpassword123";

        var hashedPassword = _passwordHasher.HashPassword(password);

        hashedPassword.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
    {
        var password = "testpassword123";

        var hashedPassword1 = _passwordHasher.HashPassword(password);
        var hashedPassword2 = _passwordHasher.HashPassword(password);

        hashedPassword1.Should().NotBe(hashedPassword2, "each hash should use a unique salt");
    }

    [Fact]
    public void HashPassword_ShouldReturnBase64String()
    {
        var password = "testpassword123";

        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValidBase64 = () => Convert.FromBase64String(hashedPassword);
        isValidBase64.Should().NotThrow("hashed password should be valid base64");
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowException()
    {
        var password = "";

        var action = () => _passwordHasher.HashPassword(password);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "testpassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hashedPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "testpassword123";
        var wrongPassword = "wrongpassword";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(wrongPassword, hashedPassword);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldThrowException()
    {
        var password = "testpassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var action = () => _passwordHasher.VerifyPassword("", hashedPassword);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VerifyPassword_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        var password = "TestPassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword("testpassword123", hashedPassword);

        result.Should().BeFalse("passwords should be case-sensitive");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("averagelengthpassword")]
    [InlineData("verylongpasswordwithmanycharacterstotest1234567890")]
    public void HashPassword_WithVariousLengths_ShouldSucceed(string password)
    {
        var hashedPassword = _passwordHasher.HashPassword(password);

        hashedPassword.Should().NotBeNullOrEmpty();
        _passwordHasher.VerifyPassword(password, hashedPassword).Should().BeTrue();
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("P@ssw0rd!")]
    [InlineData("unicode密码测试")]
    [InlineData("special!@#$%^&*()chars")]
    public void VerifyPassword_WithVariousPasswords_ShouldVerifyCorrectly(string password)
    {
        var hashedPassword = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hashedPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithSlightlyDifferentPassword_ShouldReturnFalse()
    {
        var password = "testpassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword("testpassword124", hashedPassword);

        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldIncludeSaltInOutput()
    {
        var password = "testpassword123";

        var hashedPassword = _passwordHasher.HashPassword(password);
        var hashBytes = Convert.FromBase64String(hashedPassword);

        // Hash should contain salt (16 bytes) + hash (32 bytes) = 48 bytes
        hashBytes.Length.Should().Be(48, "hash should contain 16 bytes of salt and 32 bytes of hash");
    }

    [Fact]
    public void VerifyPassword_MultipleTimes_ShouldConsistentlyReturnTrue()
    {
        var password = "testpassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        for (int i = 0; i < 5; i++)
        {
            var result = _passwordHasher.VerifyPassword(password, hashedPassword);
            result.Should().BeTrue($"verification attempt {i + 1} should succeed");
        }
    }

    [Fact]
    public void VerifyPassword_WithInvalidBase64Hash_ShouldThrowException()
    {
        var password = "testpassword123";
        var invalidHash = "not-a-valid-base64-hash!!!";

        var action = () => _passwordHasher.VerifyPassword(password, invalidHash);

        action.Should().Throw<FormatException>();
    }
}
