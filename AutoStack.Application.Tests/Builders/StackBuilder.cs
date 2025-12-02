using AutoStack.Domain.Entities;
using System.Reflection;

namespace AutoStack.Application.Tests.Builders;

/// <summary>
/// Fluent builder for creating Stack test data
/// </summary>
public class StackBuilder
{
    private string _name = "Test Stack";
    private string _description = "Test Description for Stack";
    private string _type = "FRONTEND";
    private Guid _userId = Guid.NewGuid();
    private User? _user = null;

    public StackBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public StackBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public StackBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public StackBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public StackBuilder WithUser(User user)
    {
        _user = user;
        _userId = user.Id;
        return this;
    }

    public Stack Build()
    {
        var stack = Stack.Create(_name, _description, _type, _userId);

        // If no user was explicitly set, create a default one
        if (_user == null)
        {
            _user = new UserBuilder()
                .WithUsername("testuser")
                .WithEmail("test@example.com")
                .Build();
        }

        // Use reflection to set the User navigation property
        var userProperty = typeof(Stack).GetProperty("User");
        userProperty?.SetValue(stack, _user);

        return stack;
    }
}
