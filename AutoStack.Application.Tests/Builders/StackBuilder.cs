using AutoStack.Domain.Entities;

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

    public Stack Build()
    {
        return Stack.Create(_name, _description, _type, _userId);
    }
}
