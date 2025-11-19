namespace AutoStack.Domain.Common;

public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public TId Id { get; } = default!;

    /// <summary>
    /// When this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// When this entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Protected parameterless constructor for ORM frameworks.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp. Should be called when the entity is modified.
    /// </summary>
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    #region Equality

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    #endregion
}