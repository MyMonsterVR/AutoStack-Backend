using AutoStack.Domain.Entities;

namespace AutoStack.Application.Tests.Builders;

/// <summary>
/// Fluent builder for creating Package test data
/// </summary>
public class PackageBuilder
{
    private string _name = "react";
    private string _link = "https://www.npmjs.com/package/react";
    private bool _isVerified = false;

    public PackageBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PackageBuilder WithLink(string link)
    {
        _link = link;
        return this;
    }

    public PackageBuilder AsVerified()
    {
        _isVerified = true;
        return this;
    }

    public Package Build()
    {
        return Package.Create(_name, _link, _isVerified);
    }
}
