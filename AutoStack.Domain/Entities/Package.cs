using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a software package that can be included in stacks
/// </summary>
public class Package : Entity<Guid>
{
    /// <summary>
    /// Gets the name of the package
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the URL link to the package (e.g., npm registry URL)
    /// </summary>
    public string Link { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the package has been verified by an administrator
    /// </summary>
    public bool IsVerified { get; private set; }

    // Navigation property
    private readonly List<StackInfo> _stackInfos = new();
    public IReadOnlyCollection<StackInfo> StackInfos => _stackInfos.AsReadOnly();

    // Parameterless constructor for EF Core
    private Package()
    {
    }

    private Package(Guid id, string name, string link, bool isVerified = false) : base(id)
    {
        Name = name;
        Link = link;
        IsVerified = isVerified;
    }

    /// <summary>
    /// Creates a new package with the specified properties
    /// </summary>
    /// <param name="name">The name of the package</param>
    /// <param name="link">The URL link to the package</param>
    /// <param name="isVerified">Whether the package is verified (default: false)</param>
    /// <returns>A new Package instance</returns>
    /// <exception cref="ArgumentException">Thrown when name or link is null or empty</exception>
    public static Package Create(string name, string link, bool isVerified = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Package link cannot be empty", nameof(link));

        return new Package(Guid.NewGuid(), name, link, isVerified);
    }

    /// <summary>
    /// Marks the package as verified and updates the timestamp
    /// </summary>
    public void MarkAsVerified()
    {
        IsVerified = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the package name and link, then updates the timestamp
    /// </summary>
    /// <param name="name">The new name for the package</param>
    /// <param name="link">The new URL link for the package</param>
    /// <exception cref="ArgumentException">Thrown when name or link is null or empty</exception>
    public void Update(string name, string link)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Package link cannot be empty", nameof(link));

        Name = name;
        Link = link;
        UpdateTimestamp();
    }
}
