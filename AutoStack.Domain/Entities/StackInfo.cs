using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class StackInfo : Entity<Guid>
{
    public Guid StackId { get; private set; }
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

    public static StackInfo Create(Guid stackId, Guid packageId)
    {
        return new StackInfo(Guid.NewGuid(), stackId, packageId);
    }
}
