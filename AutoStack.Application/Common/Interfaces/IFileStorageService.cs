namespace AutoStack.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);
    Task DeleteAvatarAsync(string fileUrl, CancellationToken cancellationToken);
}
