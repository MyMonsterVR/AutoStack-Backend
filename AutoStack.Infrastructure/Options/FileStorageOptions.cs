namespace AutoStack.Infrastructure.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string AvatarPath { get; set; } = "uploads/avatars";
    public string BaseUrl { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 5242880; // 5MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
}
