using AutoStack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AutoStack.Domain.Tests.Entities;

public class StackInfoTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateStackInfo()
    {
        var stackId = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var stackInfo = StackInfo.Create(stackId, packageId);

        stackInfo.Should().NotBeNull();
        stackInfo.StackId.Should().Be(stackId);
        stackInfo.PackageId.Should().Be(packageId);
        stackInfo.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldGenerateNewGuid()
    {
        var stackId1 = Guid.NewGuid();
        var packageId1 = Guid.NewGuid();
        var stackId2 = Guid.NewGuid();
        var packageId2 = Guid.NewGuid();

        var stackInfo1 = StackInfo.Create(stackId1, packageId1);
        var stackInfo2 = StackInfo.Create(stackId2, packageId2);

        stackInfo1.Id.Should().NotBeEmpty();
        stackInfo2.Id.Should().NotBeEmpty();
        stackInfo1.Id.Should().NotBe(stackInfo2.Id);
    }

    [Fact]
    public void Create_ShouldSetTimestamps()
    {
        var stackId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var stackInfo = StackInfo.Create(stackId, packageId);

        var after = DateTime.UtcNow;
        stackInfo.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        stackInfo.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        stackInfo.CreatedAt.Should().BeCloseTo(stackInfo.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Create_WithSameStackDifferentPackages_ShouldCreateMultipleStackInfos()
    {
        var stackId = Guid.NewGuid();
        var packageId1 = Guid.NewGuid();
        var packageId2 = Guid.NewGuid();

        var stackInfo1 = StackInfo.Create(stackId, packageId1);
        var stackInfo2 = StackInfo.Create(stackId, packageId2);

        stackInfo1.StackId.Should().Be(stackId);
        stackInfo2.StackId.Should().Be(stackId);
        stackInfo1.PackageId.Should().NotBe(stackInfo2.PackageId);
        stackInfo1.Id.Should().NotBe(stackInfo2.Id);
    }

    [Fact]
    public void Create_WithDifferentStacksSamePackage_ShouldCreateMultipleStackInfos()
    {
        var stackId1 = Guid.NewGuid();
        var stackId2 = Guid.NewGuid();
        var packageId = Guid.NewGuid();

        var stackInfo1 = StackInfo.Create(stackId1, packageId);
        var stackInfo2 = StackInfo.Create(stackId2, packageId);

        stackInfo1.PackageId.Should().Be(packageId);
        stackInfo2.PackageId.Should().Be(packageId);
        stackInfo1.StackId.Should().NotBe(stackInfo2.StackId);
        stackInfo1.Id.Should().NotBe(stackInfo2.Id);
    }
}
