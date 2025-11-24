using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class User : Entity<Guid>
{
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; }  = string.Empty;
    public string PasswordHash { get; set; }  = string.Empty;
    
    public User()
    {}

    private User(Guid id, string email, string username) : base(id)
    {
        Email = email;
        Username = username;
    }

    public static User CreateUser(string email, string username)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        var user = new User(Guid.NewGuid(), email, username);
        return user;
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}