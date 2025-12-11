using System.Text.RegularExpressions;
using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a user account in the system
/// </summary>
public partial class User : Entity<Guid>
{
    /// <summary>
    /// Gets the user's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets the user's unique username
    /// </summary>
    public string Username { get; set; }  = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password for the user
    /// </summary>
    public string PasswordHash { get; set; }  = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's avatar url
    /// </summary>
    public string AvatarUrl  { get; set; }  = string.Empty;

    /// <summary>
    /// Gets the status of 2FA
    /// </summary>
    public bool TwoFactorEnabled { get; private set; }

    /// <summary>
    /// Gets the secret key for 2FA
    /// </summary>
    public string? TwoFactorSecretKey { get; private set; }

    /// <summary>
    /// Gets the date 2FA was enabled
    /// </summary>
    public DateTime? TwoFactorEnabledAt { get; private set; }

    public string? PasswordResetToken { get; private set; }

    public DateTime? PasswordResetTokenExpiry { get; private set; }

    /// <summary>
    /// Gets whether the user's email has been verified
    /// </summary>
    public bool EmailVerified { get; private set; } = false;

    /// <summary>
    /// Gets the email verification code
    /// </summary>
    public string? EmailVerificationCode { get; private set; }

    /// <summary>
    /// Gets when the email verification code expires
    /// </summary>
    public DateTime? EmailVerificationCodeExpiry { get; private set; }

    /// <summary>
    /// Gets when the email was verified
    /// </summary>
    public DateTime? EmailVerifiedAt { get; private set; }

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
        user.SetDefaultAvatar();
        return user;
    }

    /// <summary>
    /// Sets a default avatar URL based on the user's username
    /// </summary>
    private void SetDefaultAvatar()
    {
        // Generate avatar using dicebear service with username
        var encodedUsername = Uri.EscapeDataString(Username);
        AvatarUrl = $"https://api.dicebear.com/9.x/thumbs/svg?seed={encodedUsername}";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the username for the user and updates the timestamp
    /// </summary>
    /// <param name="newUsername">The username of the user</param>
    /// <exception cref="ArgumentException">Thrown when username is null or whitespace</exception>
    public void SetUsername(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(newUsername));
        }
        
        Username = newUsername;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the email for the user and updates the timestamp
    /// </summary>
    /// <param name="email">The email of the user</param>
    /// <exception cref="ArgumentException">Thrown when email is null or whitespace</exception>
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }
        
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the hashed password for the user and updates the timestamp
    /// </summary>
    /// <param name="passwordHash">The hashed password to set</param>
    /// <exception cref="ArgumentException">Thrown when password hash is null or whitespace</exception>
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="avatarUrl">The url of the avatar</param>
    /// <exception cref="ArgumentException">Thrown when avatar url is null or whitespace</exception>
    public void SetAvatarUrl(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            throw new ArgumentException("Avatar url cannot be null or empty", nameof(avatarUrl));
        }

        if (!IsValidUrl(avatarUrl))
        {
            throw new ArgumentException("Invalid avatar url", nameof(avatarUrl));
        }

        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a url is in a valid url format (No special characters)
    /// </summary>
    /// <param name="url">The url of the user's avatar</param>
    /// <returns>true if url is valid; else false</returns>
    private static bool IsValidUrl(string url)
    {
        var regex = ValidUrlPattern();
        return regex.IsMatch(url);
    }

    /*
     to be fair, url will always be sat to our own domain,
     but this regex opens up for the opportunity to have custom user urls,
     as long as they don't include special characters
    */
    [GeneratedRegex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)*(?::[\d]+)?(?:[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-DK")]
    private static partial Regex ValidUrlPattern();

    public void EnableTwoFactorAuthentication(string twoFactorSecretKey)
    {
        if (string.IsNullOrWhiteSpace(twoFactorSecretKey))
        {
            throw new ArgumentException("Two factor credentials cannot be null or empty", nameof(twoFactorSecretKey));
        }
        
        TwoFactorEnabled = true;
        TwoFactorSecretKey = twoFactorSecretKey;
        TwoFactorEnabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactorAuthentication()
    {
        TwoFactorEnabled = false;
        TwoFactorSecretKey = null;
        TwoFactorEnabledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetPasswordResetToken(string resetToken, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(resetToken))
        {
            throw new ArgumentException("Password reset token cannot be null or empty", nameof(resetToken));
        }
        
        PasswordResetToken = resetToken;
        PasswordResetTokenExpiry = expiry;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    /// <summary>
    /// Sets the email verification code and expiry
    /// </summary>
    /// <param name="code">The 6-digit verification code</param>
    /// <param name="expiry">When the code expires</param>
    /// <exception cref="ArgumentException">Thrown when code is null or empty</exception>
    public void SetEmailVerificationCode(string code, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Verification code cannot be null or empty", nameof(code));
        }

        EmailVerificationCode = code;
        EmailVerificationCodeExpiry = expiry;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the email as verified and clears the verification code
    /// </summary>
    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerificationCode = null;
        EmailVerificationCodeExpiry = null;
        EmailVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears the email verification code without marking as verified
    /// </summary>
    public void ClearEmailVerificationCode()
    {
        EmailVerificationCode = null;
        EmailVerificationCodeExpiry = null;
        UpdatedAt = DateTime.UtcNow;
    }
}