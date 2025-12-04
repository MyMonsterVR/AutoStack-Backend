using AutoStack.Domain.Entities;
using AutoStack.Infrastructure.Persistence;
using AutoStack.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoStack.Infrastructure.Tests.Repositories;

public class StackRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StackRepository _repository;

    public StackRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new StackRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingStack_ShouldReturnStack()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(stack.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(stack.Id);
        result.Name.Should().Be("Test Stack");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentStack_ShouldReturnNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithInfoAsync_WithExistingStack_ShouldReturnStackWithRelations()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "BACKEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByIdWithInfoAsync(stack.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(stack.Id);
        result.User.Should().NotBeNull();
        result.Packages.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdWithInfoAsync_WithNonExistentStack_ShouldReturnNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdWithInfoAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingStacks_ShouldReturnUserStacks()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack1 = Stack.Create("Stack 1", "Description 1", "FRONTEND", user.Id);
        var stack2 = Stack.Create("Stack 2", "Description 2", "BACKEND", user.Id);
        await _dbContext.Stacks.AddRangeAsync(stack1, stack2);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(user.Id);

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "Stack 1");
        result.Should().Contain(s => s.Name == "Stack 2");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoStacks_ShouldReturnEmpty()
    {
        var userId = Guid.NewGuid();

        var result = await _repository.GetByUserIdAsync(userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldOnlyReturnStacksForSpecificUser()
    {
        var user1 = User.CreateUser("test1@example.com", "testuser1");
        var user2 = User.CreateUser("test2@example.com", "testuser2");
        await _dbContext.Users.AddRangeAsync(user1, user2);

        var stack1 = Stack.Create("User1 Stack", "Description", "FRONTEND", user1.Id);
        var stack2 = Stack.Create("User2 Stack", "Description", "BACKEND", user2.Id);
        await _dbContext.Stacks.AddRangeAsync(stack1, stack2);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(user1.Id);

        result.Should().HaveCount(1);
        result.Should().Contain(s => s.Name == "User1 Stack");
        result.Should().NotContain(s => s.Name == "User2 Stack");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleStacks_ShouldReturnAll()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack1 = Stack.Create("Stack 1", "Description 1", "FRONTEND", user.Id);
        var stack2 = Stack.Create("Stack 2", "Description 2", "BACKEND", user.Id);
        var stack3 = Stack.Create("Stack 3", "Description 3", "FULLSTACK", user.Id);
        await _dbContext.Stacks.AddRangeAsync(stack1, stack2, stack3);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithNoStacks_ShouldReturnEmpty()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddStackToContext()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _repository.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var retrievedStack = await _dbContext.Stacks.FindAsync(stack.Id);
        retrievedStack.Should().NotBeNull();
        retrievedStack!.Name.Should().Be("Test Stack");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStack()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var initialDownloads = stack.Downloads;
        stack.IncrementDownloads();
        await _repository.UpdateAsync(stack);
        await _dbContext.SaveChangesAsync();

        var updatedStack = await _dbContext.Stacks.FindAsync(stack.Id);
        updatedStack!.Downloads.Should().Be(initialDownloads + 1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var originalTimestamp = stack.UpdatedAt;
        await Task.Delay(10);

        stack.IncrementDownloads();
        await _repository.UpdateAsync(stack);
        await _dbContext.SaveChangesAsync();

        var updatedStack = await _dbContext.Stacks.FindAsync(stack.Id);
        updatedStack!.UpdatedAt.Should().BeAfter(originalTimestamp);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveStack()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        await _repository.DeleteAsync(stack);
        await _dbContext.SaveChangesAsync();

        var deletedStack = await _dbContext.Stacks.FindAsync(stack.Id);
        deletedStack.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingStack_ShouldReturnTrue()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack = Stack.Create("Test Stack", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.ExistsAsync(stack.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentStack_ShouldReturnFalse()
    {
        var nonExistentId = Guid.NewGuid();

        var exists = await _repository.ExistsAsync(nonExistentId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetStacksPagedAsync_ShouldReturnCorrectPage()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        for (int i = 1; i <= 10; i++)
        {
            var stack = Stack.Create($"Stack {i}", $"Description {i}", "FRONTEND", user.Id);
            await _dbContext.Stacks.AddAsync(stack);
        }
        await _dbContext.SaveChangesAsync();

        var (stacks, totalCount) = await _repository.GetStacksPagedAsync(
            pageNumber: 1,
            pageSize: 5,
            stackType: null,
            sortBy: "popularity",
            sortDescending: false);

        stacks.Should().HaveCount(5);
        totalCount.Should().Be(10);
    }

    [Fact]
    public async Task GetStacksPagedAsync_WithStackTypeFilter_ShouldReturnFilteredStacks()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var frontendStack = Stack.Create("Frontend Stack", "Description", "FRONTEND", user.Id);
        var backendStack = Stack.Create("Backend Stack", "Description", "BACKEND", user.Id);
        await _dbContext.Stacks.AddRangeAsync(frontendStack, backendStack);
        await _dbContext.SaveChangesAsync();

        var (stacks, totalCount) = await _repository.GetStacksPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            stackType: "FRONTEND",
            sortBy: "popularity",
            sortDescending: false);

        stacks.Should().HaveCount(1);
        stacks.Should().Contain(s => s.Type == "FRONTEND");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetStacksPagedAsync_WithPopularitySortDescending_ShouldReturnCorrectOrder()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack1 = Stack.Create("Stack 1", "Description", "FRONTEND", user.Id);
        stack1.IncrementDownloads();
        stack1.IncrementDownloads();

        var stack2 = Stack.Create("Stack 2", "Description", "FRONTEND", user.Id);
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();

        var stack3 = Stack.Create("Stack 3", "Description", "FRONTEND", user.Id);
        stack3.IncrementDownloads();

        await _dbContext.Stacks.AddRangeAsync(stack1, stack2, stack3);
        await _dbContext.SaveChangesAsync();

        var (stacks, _) = await _repository.GetStacksPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            stackType: null,
            sortBy: "popularity",
            sortDescending: true);

        var stacksList = stacks.ToList();
        stacksList[0].Downloads.Should().Be(5);
        stacksList[1].Downloads.Should().Be(2);
        stacksList[2].Downloads.Should().Be(1);
    }

    [Fact]
    public async Task GetStacksPagedAsync_WithCreatedDateSort_ShouldReturnCorrectOrder()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        var stack1 = Stack.Create("Stack 1", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack1);
        await _dbContext.SaveChangesAsync();

        await Task.Delay(10);

        var stack2 = Stack.Create("Stack 2", "Description", "FRONTEND", user.Id);
        await _dbContext.Stacks.AddAsync(stack2);
        await _dbContext.SaveChangesAsync();

        var (stacks, _) = await _repository.GetStacksPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            stackType: null,
            sortBy: "createddate",
            sortDescending: false);

        var stacksList = stacks.ToList();
        stacksList[0].Name.Should().Be("Stack 1");
        stacksList[1].Name.Should().Be("Stack 2");
    }

    [Fact]
    public async Task GetStacksPagedAsync_SecondPage_ShouldReturnCorrectItems()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);

        for (int i = 1; i <= 10; i++)
        {
            var stack = Stack.Create($"Stack {i}", $"Description {i}", "FRONTEND", user.Id);
            await _dbContext.Stacks.AddAsync(stack);
        }
        await _dbContext.SaveChangesAsync();

        var (stacks, totalCount) = await _repository.GetStacksPagedAsync(
            pageNumber: 2,
            pageSize: 3,
            stackType: null,
            sortBy: "popularity",
            sortDescending: false);

        stacks.Should().HaveCount(3);
        totalCount.Should().Be(10);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
