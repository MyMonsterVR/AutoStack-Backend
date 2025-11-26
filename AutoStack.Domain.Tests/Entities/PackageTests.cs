using AutoStack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AutoStack.Domain.Tests.Entities;

public class PackageTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreatePackage()
    {
        var name = "react";
        var link = "https://www.npmjs.com/package/react";

        var package = Package.Create(name, link);

        package.Should().NotBeNull();
        package.Name.Should().Be(name);
        package.Link.Should().Be(link);
        package.IsVerified.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        var link = "https://www.npmjs.com/package/react";

        Action act = () => Package.Create(invalidName!, link);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Package name cannot be empty*")
            .WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyLink_ShouldThrowArgumentException(string? invalidLink)
    {
        var name = "react";

        Action act = () => Package.Create(name, invalidLink!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Package link cannot be empty*")
            .WithParameterName("link");
    }

    [Fact]
    public void Create_WithVerifiedTrue_ShouldSetIsVerified()
    {
        var name = "react";
        var link = "https://www.npmjs.com/package/react";

        var package = Package.Create(name, link, isVerified: true);

        package.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void Create_DefaultIsVerifiedShouldBeFalse()
    {
        var name = "react";
        var link = "https://www.npmjs.com/package/react";

        var package = Package.Create(name, link);

        package.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void MarkAsVerified_ShouldSetIsVerifiedToTrue()
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");

        package.MarkAsVerified();

        package.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void MarkAsVerified_ShouldUpdateTimestamp()
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");
        var originalUpdatedAt = package.UpdatedAt;
        Thread.Sleep(10);

        package.MarkAsVerified();

        package.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateNameAndLink()
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");
        var newName = "vue";
        var newLink = "https://www.npmjs.com/package/vue";

        package.Update(newName, newLink);

        package.Name.Should().Be(newName);
        package.Link.Should().Be(newLink);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Update_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");
        var link = "https://www.npmjs.com/package/vue";

        Action act = () => package.Update(invalidName!, link);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Package name cannot be empty*")
            .WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Update_WithEmptyLink_ShouldThrowArgumentException(string? invalidLink)
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");
        var name = "vue";

        Action act = () => package.Update(name, invalidLink!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Package link cannot be empty*")
            .WithParameterName("link");
    }

    [Fact]
    public void Update_ShouldUpdateTimestamp()
    {
        var package = Package.Create("react", "https://www.npmjs.com/package/react");
        var originalUpdatedAt = package.UpdatedAt;
        Thread.Sleep(10);

        package.Update("vue", "https://www.npmjs.com/package/vue");

        package.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Create_ShouldSetTimestamps()
    {
        var before = DateTime.UtcNow;

        var package = Package.Create("react", "https://www.npmjs.com/package/react");

        var after = DateTime.UtcNow;
        package.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        package.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
