using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a technology stack created by a user containing multiple packages
/// </summary>
public class Stack : Entity<Guid>
{
    /// <summary>
    /// Gets the name of the stack
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of the stack
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the type of the stack (e.g., "frontend", "backend", "fullstack")
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the number of times this stack has been downloaded
    /// </summary>
    public int Downloads { get; private set; }

    /// <summary>
    /// Gets the ID of the user who created this stack
    /// </summary>
    public Guid UserId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    private readonly List<StackInfo> _stackInfo = new();
    public IReadOnlyCollection<StackInfo> StackInfo => _stackInfo.AsReadOnly();

    // Parameterless constructor for EF Core
    private Stack()
    {
    }

    private Stack(Guid id, string name, string description, string type, Guid userId) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        Downloads = 0;
        UserId = userId;
    }

    /// <summary>
    /// Creates a new stack with the specified properties
    /// </summary>
    /// <param name="name">The name of the stack</param>
    /// <param name="description">The description of the stack</param>
    /// <param name="type">The type of the stack</param>
    /// <param name="userId">The ID of the user creating the stack</param>
    /// <returns>A new Stack instance</returns>
    /// <exception cref="ArgumentException">Thrown when any required parameter is null or empty</exception>
    public static Stack Create(string name, string description, string type, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Stack name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Stack description cannot be empty", nameof(description));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Stack type cannot be empty", nameof(type));

        return new Stack(Guid.NewGuid(), name, description, type, userId);
    }

    /// <summary>
    /// Adds a StackInfo entry linking a package to this stack
    /// </summary>
    /// <param name="stackInfo">The StackInfo to add</param>
    /// <exception cref="ArgumentNullException">Thrown when stackInfo is null</exception>
    public void AddStackInfo(StackInfo stackInfo)
    {
        if (stackInfo == null)
            throw new ArgumentNullException(nameof(stackInfo));

        _stackInfo.Add(stackInfo);
        UpdateTimestamp();
    }

    /// <summary>
    /// Increments the download count for this stack and updates the timestamp
    /// </summary>
    public void IncrementDownloads()
    {
        Downloads++;
        UpdateTimestamp();
    }
}