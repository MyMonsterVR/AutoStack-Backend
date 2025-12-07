using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a one-time recovery code for 2FA backup access
/// </summary>
public class RecoveryCode : Entity<Guid>
{
    /// <summary>
    /// Gets the ID of the user this recovery code belongs to
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the hashed recovery code (stored hashed, never plain text)
    /// </summary>
    public string CodeHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether this code has been used
    /// </summary>
    public bool IsUsed { get; private set; }

    /// <summary>
    /// Gets when this code was used
    /// </summary>
    public DateTime? UsedAt { get; private set; }

    public RecoveryCode()
    {}

    private RecoveryCode(Guid id, Guid userId, string codeHash) : base(id)
    {
        UserId = userId;
        CodeHash = codeHash;
        IsUsed = false;
    }

    /// <summary>
    /// Creates a new recovery code
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="codeHash">The hashed code</param>
    /// <returns>A new RecoveryCode instance</returns>
    public static RecoveryCode Create(Guid userId, string codeHash)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException("Code hash cannot be null or empty", nameof(codeHash));

        return new RecoveryCode(Guid.NewGuid(), userId, codeHash);
    }

    /// <summary>
    /// Marks this recovery code as used
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}