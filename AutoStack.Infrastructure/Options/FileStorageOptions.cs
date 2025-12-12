namespace AutoStack.Infrastructure.Options;

/// <summary>
/// Configuration options for file storage (avatar uploads)
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Gets or sets the path where avatars are stored
    /// </summary>
    public string AvatarPath { get; set; } = "uploads/avatars";

    /// <summary>
    /// Gets or sets the base URL for accessing uploaded files
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum file size in bytes (default: 5MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5242880; // 5MB

    /// <summary>
    /// Gets or sets the allowed file extensions for uploads
    /// </summary>
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
}
