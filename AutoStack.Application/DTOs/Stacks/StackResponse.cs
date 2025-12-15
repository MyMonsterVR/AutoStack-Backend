using System.Text.Json.Serialization;

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
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the stack
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the stack
    /// </summary>
    [JsonPropertyName("type")]
    public StackTypeResponse Type { get; set; }

    /// <summary>
    /// Gets or sets the number of times the stack has been downloaded
    /// </summary>
    public int Downloads { get; set; }

    /// <summary>
    /// Gets or sets the number of upvotes this stack has received
    /// </summary>
    public int UpvoteCount { get; set; }

    /// <summary>
    /// Gets or sets the number of downvotes this stack has received
    /// </summary>
    public int DownvoteCount { get; set; }

    /// <summary>
    /// Gets or sets the current user's vote on this stack (null if not voted, true if upvoted, false if downvoted)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? UserVote { get; set; }

    /// <summary>
    /// Gets or sets the list of packages included in the stack
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<PackageResponse>? Packages { get; set; }
    
    /// <summary>
    /// Date the stack was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    // USER INFO
    /// <summary>
    /// The id of the stack owner
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Stack owners username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Stack owners avatar image link
    /// </summary>
    public string? UserAvatarUrl { get; set; }
}