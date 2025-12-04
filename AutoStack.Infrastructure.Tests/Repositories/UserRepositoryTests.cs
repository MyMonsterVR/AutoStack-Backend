using AutoStack.Domain.Entities;
using AutoStack.Infrastructure.Persistence;
using AutoStack.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoStack.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToContext()
    {
        var user = User.CreateUser("test@example.com", "testuser");

        await _repository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var retrievedUser = await _dbContext.Users.FindAsync(user.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task AddAsync_WithMultipleUsers_ShouldAddAll()
    {
        var user1 = User.CreateUser("test1@example.com", "testuser1");
        var user2 = User.CreateUser("test2@example.com", "testuser2");

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _dbContext.SaveChangesAsync();

        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        user.SetUsername("updateduser");
        await _repository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Username.Should().Be("updateduser");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var originalTimestamp = user.UpdatedAt;

        await Task.Delay(10);

        user.SetUsername("updateduser");
        await _repository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser!.UpdatedAt.Should().BeAfter(originalTimestamp);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        await _repository.DeleteAsync(user);
        await _dbContext.SaveChangesAsync();

        var deletedUser = await _dbContext.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.ExistsAsync(user.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        var nonExistentId = Guid.NewGuid();

        var exists = await _repository.ExistsAsync(nonExistentId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExists_WithExistingEmail_ShouldReturnTrue()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.EmailExists("test@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExists_WithNonExistentEmail_ShouldReturnFalse()
    {
        var exists = await _repository.EmailExists("nonexistent@example.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExists_ShouldBeCaseSensitive()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.EmailExists("TEST@EXAMPLE.COM");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExists_WithExistingUsername_ShouldReturnTrue()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.UsernameExists("testuser");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameExists_WithNonExistentUsername_ShouldReturnFalse()
    {
        var exists = await _repository.UsernameExists("nonexistentuser");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExists_ShouldBeCaseSensitive()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await _repository.UsernameExists("TESTUSER");

        exists.Should().BeFalse();
    }


    [Fact]
    public async Task GetByIdAsync_WithMultipleUsers_ShouldReturnCorrectUser()
    {
        var user1 = User.CreateUser("test1@example.com", "testuser1");
        var user2 = User.CreateUser("test2@example.com", "testuser2");
        var user3 = User.CreateUser("test3@example.com", "testuser3");

        await _dbContext.Users.AddRangeAsync(user1, user2, user3);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user2.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user2.Id);
        result.Username.Should().Be("testuser2");
    }

    [Fact]
    public async Task UpdateAsync_WithEmailChange_ShouldPersist()
    {
        var user = User.CreateUser("old@example.com", "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        user.SetEmail("new@example.com");
        await _repository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser!.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WithPasswordChange_ShouldPersist()
    {
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetPassword("old_password_hash");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        user.SetPassword("new_password_hash");
        await _repository.UpdateAsync(user);
        await _dbContext.SaveChangesAsync();

        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser!.PasswordHash.Should().Be("new_password_hash");
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
