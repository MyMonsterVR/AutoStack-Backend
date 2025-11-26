using AutoStack.Domain.Entities;

namespace AutoStack.Application.Tests.Builders;

/// <summary>
/// Fluent builder for creating User test data
/// </summary>
public class UserBuilder
{
    private string _email = "test@example.com";
    private string _username = "testuser";

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public User Build()
    {
        return User.CreateUser(_email, _username);
    }
}
