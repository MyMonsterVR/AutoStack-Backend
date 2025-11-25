using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class Stack : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public int Downloads { get; private set; }
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

    public void AddStackInfo(StackInfo stackInfo)
    {
        if (stackInfo == null)
            throw new ArgumentNullException(nameof(stackInfo));

        _stackInfo.Add(stackInfo);
        UpdateTimestamp();
    }

    public void IncrementDownloads()
    {
        Downloads++;
        UpdateTimestamp();
    }
}