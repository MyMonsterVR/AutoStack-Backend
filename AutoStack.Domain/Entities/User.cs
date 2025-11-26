using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a user account in the system
/// </summary>
public class User : Entity<Guid>
{
    /// <summary>
    /// Gets the user's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's unique username
    /// </summary>
    public string Username { get; init; }  = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password for the user
    /// </summary>
    public string PasswordHash { get; set; }  = string.Empty;
    
    public User()
    {}

    private User(Guid id, string email, string username) : base(id)
    {
        Email = email;
        Username = username;
    }

    /// <summary>
    /// Creates a new user with the specified email and username
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="username">The user's username</param>
    /// <returns>A new User instance</returns>
    /// <exception cref="ArgumentException">Thrown when email or username is null or empty</exception>
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

    /// <summary>
    /// Sets the hashed password for the user and updates the timestamp
    /// </summary>
    /// <param name="passwordHash">The hashed password to set</param>
    /// <exception cref="ArgumentException">Thrown when password hash is null or empty</exception>
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}