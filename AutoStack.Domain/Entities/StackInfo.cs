using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a many-to-many relationship between stacks and packages
/// </summary>
public class StackInfo : Entity<Guid>
{
    /// <summary>
    /// Gets the ID of the stack
    /// </summary>
    public Guid StackId { get; private set; }

    /// <summary>
    /// Gets the ID of the package
    /// </summary>
    public Guid PackageId { get; private set; }

    // Navigation properties
    public Stack Stack { get; private set; } = null!;
    public Package Package { get; private set; } = null!;

    // Parameterless constructor for EF Core
    private StackInfo()
    {
    }

    private StackInfo(Guid id, Guid stackId, Guid packageId) : base(id)
    {
        StackId = stackId;
        PackageId = packageId;
    }

    /// <summary>
    /// Creates a new StackInfo linking a stack and a package
    /// </summary>
    /// <param name="stackId">The ID of the stack</param>
    /// <param name="packageId">The ID of the package</param>
    /// <returns>A new StackInfo instance</returns>
    public static StackInfo Create(Guid stackId, Guid packageId)
    {
        return new StackInfo(Guid.NewGuid(), stackId, packageId);
    }
}
