namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Response DTO containing technology stack information with its packages
/// </summary>
public class StackResponse
{
    /// <summary>
    /// Gets or sets the id of the stack
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the stack
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the stack
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the type of the stack
    /// </summary>
    public StackType Type { get; set; }

    /// <summary>
    /// Gets or sets the number of times the stack has been downloaded
    /// </summary>
    public int Downloads { get; set; }

    /// <summary>
    /// Gets or sets the list of packages included in the stack
    /// </summary>
    public List<StackInfoResponse> StackInfo { get; set; }
}