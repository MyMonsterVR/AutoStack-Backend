using AutoStack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AutoStack.Domain.Tests.Entities;

public class StackTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateStack()
    {
        var name = "React Stack";
        var description = "A modern React stack for building UIs";
        var type = "Frontend";
        var userId = Guid.NewGuid();

        var stack = Stack.Create(name, description, type, userId);

        stack.Should().NotBeNull();
        stack.Name.Should().Be(name);
        stack.Description.Should().Be(description);
        stack.Type.Should().Be(type);
        stack.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        var description = "Valid description";
        var type = "Frontend";
        var userId = Guid.NewGuid();

        Action act = () => Stack.Create(invalidName!, description, type, userId);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Stack name cannot be empty*")
            .WithParameterName("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        var name = "Valid Stack";
        var type = "Frontend";
        var userId = Guid.NewGuid();

        Action act = () => Stack.Create(name, invalidDescription!, type, userId);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Stack description cannot be empty*")
            .WithParameterName("description");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyType_ShouldThrowArgumentException(string? invalidType)
    {
        var name = "Valid Stack";
        var description = "Valid description";
        var userId = Guid.NewGuid();

        Action act = () => Stack.Create(name, description, invalidType!, userId);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Stack type cannot be empty*")
            .WithParameterName("type");
    }

    [Fact]
    public void Create_ShouldInitializeDownloadsToZero()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());

        stack.Downloads.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldSetTimestamps()
    {
        var before = DateTime.UtcNow;

        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());

        var after = DateTime.UtcNow;
        stack.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        stack.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        stack.CreatedAt.Should().BeCloseTo(stack.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void AddStackInfo_WithValidStackInfo_ShouldAddToPackages()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());
        var packageId = Guid.NewGuid();
        var stackInfo = StackInfo.Create(stack.Id, packageId);

        stack.AddStackInfo(stackInfo);

        stack.Packages.Should().ContainSingle();
        stack.Packages.First().Should().Be(stackInfo);
    }

    [Fact]
    public void AddStackInfo_WithNullStackInfo_ShouldThrowArgumentNullException()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());

        Action act = () => stack.AddStackInfo(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stackInfo");
    }

    [Fact]
    public void AddStackInfo_ShouldUpdateTimestamp()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());
        var originalUpdatedAt = stack.UpdatedAt;
        Thread.Sleep(10);

        var stackInfo = StackInfo.Create(stack.Id, Guid.NewGuid());

        stack.AddStackInfo(stackInfo);

        stack.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void IncrementDownloads_ShouldIncreaseCount()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());
        var initialDownloads = stack.Downloads;

        stack.IncrementDownloads();

        stack.Downloads.Should().Be(initialDownloads + 1);
    }

    [Fact]
    public void IncrementDownloads_ShouldUpdateTimestamp()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());
        var originalUpdatedAt = stack.UpdatedAt;
        Thread.Sleep(10);

        stack.IncrementDownloads();

        stack.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void IncrementDownloads_CalledMultipleTimes_ShouldIncrementCorrectly()
    {
        var stack = Stack.Create("Test", "Description here", "Frontend", Guid.NewGuid());

        stack.IncrementDownloads();
        stack.IncrementDownloads();
        stack.IncrementDownloads();

        stack.Downloads.Should().Be(3);
    }
}
