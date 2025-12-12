namespace AutoStack.Application.Common.Interfaces;

/// <summary>
/// Service for managing file storage operations (avatars, uploads, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a user avatar to storage and returns the URL
    /// </summary>
    /// <param name="fileStream">The file stream containing the avatar image</param>
    /// <param name="fileName">The name of the file</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the uploaded avatar</returns>
    Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an avatar from storage
    /// </summary>
    /// <param name="fileUrl">The URL of the avatar to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAvatarAsync(string fileUrl, CancellationToken cancellationToken);
}
