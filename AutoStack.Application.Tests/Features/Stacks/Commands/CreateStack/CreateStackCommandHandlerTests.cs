using AutoStack.Application.DTOs.Stacks;
using AutoStack.Application.Features.Stacks.Commands.CreateStack;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Stacks.Commands.CreateStack;

public class CreateStackCommandHandlerTests : CommandHandlerTestBase
{
    private readonly CreateStackCommandHandler _handler;

    public CreateStackCommandHandlerTests()
    {
        _handler = new CreateStackCommandHandler(
            MockStackRepository.Object,
            MockUserRepository.Object,
            MockPackageRepository.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallRepositoryMethodsAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Package?)null);

        MockPackageRepository.Setup(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdStack = new StackBuilder()
            .WithName(command.Name)
            .WithDescription(command.Description)
            .Build();

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdStack);

        var createdUser = new UserBuilder().Build();
        MockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        MockPackageRepository.Verify(r => r.GetByLinkAsync(
            "https://www.npmjs.com/package/react",
            It.IsAny<CancellationToken>()), Times.Once);

        MockPackageRepository.Verify(r => r.AddAsync(
            It.Is<Package>(p => p.Name == "react" && p.Link == "https://www.npmjs.com/package/react"),
            It.IsAny<CancellationToken>()), Times.Once);

        MockStackRepository.Verify(r => r.AddAsync(
            It.Is<Stack>(s => s.Name == command.Name && s.Description == command.Description),
            It.IsAny<CancellationToken>()), Times.Once);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithoutUserId_ShouldReturnFailure()
    {
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User ID is required");

        MockStackRepository.Verify(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingPackages_ShouldReusePackages()
    {
        var userId = Guid.NewGuid();
        var existingPackage = new PackageBuilder()
            .WithName("react")
            .WithLink("https://www.npmjs.com/package/react")
            .Build();

        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(
                "https://www.npmjs.com/package/react",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPackage);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdStack = new StackBuilder()
            .WithName(command.Name)
            .WithDescription(command.Description)
            .Build();

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdStack);

        var createdUser = new UserBuilder().Build();
        MockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);
        
        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        MockPackageRepository.Verify(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNewPackages_ShouldCreateNewPackages()
    {
        var userId = Guid.NewGuid();
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Package?)null);

        MockPackageRepository.Setup(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdStack = new StackBuilder().Build();
        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdStack);

        var createdUser = new UserBuilder().Build();
        MockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        MockPackageRepository.Verify(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicatePackages_ShouldDeduplicateByLink()
    {
        var userId = Guid.NewGuid();
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react"),
                new("react-duplicate", "https://www.npmjs.com/package/react"),
                new("react-dom", "https://www.npmjs.com/package/react-dom")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Package?)null);

        MockPackageRepository.Setup(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdStack = new StackBuilder().Build();
        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdStack);

        var createdUser = new UserBuilder().Build();
        MockUserRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);
        
        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        MockPackageRepository.Verify(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var userId = Guid.NewGuid();
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Package?)null);

        MockPackageRepository.Setup(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdStack = new StackBuilder().Build();
        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdStack);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLoadFails_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            },
            UserId: userId
        );

        MockPackageRepository.Setup(r => r.GetByLinkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Package?)null);

        MockPackageRepository.Setup(r => r.AddAsync(It.IsAny<Package>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.AddAsync(It.IsAny<Stack>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stack?)null);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to load created stack");
    }
}
