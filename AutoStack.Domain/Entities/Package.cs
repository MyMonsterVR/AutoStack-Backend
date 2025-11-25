using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class Package : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Link { get; private set; } = string.Empty;
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

    public static Package Create(string name, string link, bool isVerified = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Package name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(link))
            throw new ArgumentException("Package link cannot be empty", nameof(link));

        return new Package(Guid.NewGuid(), name, link, isVerified);
    }

    // Admin function to verify a package
    public void MarkAsVerified()
    {
        IsVerified = true;
        UpdateTimestamp();
    }

    // Update package details
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
