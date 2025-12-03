using AutoStack.Application.Common.Interfaces;
using AutoStack.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AutoStack.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("FileStorage:BaseUrl is required in configuration");
        }

        EnsureDirectoryExists();
    }

    public async Task<string> UploadAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_options.AllowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _options.AllowedExtensions)}");
        }

        if (fileStream.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / 1024 / 1024}MB");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_options.AvatarPath, uniqueFileName);

        await using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
        }

        var avatarUrl = $"{_options.BaseUrl}/uploads/avatars/{uniqueFileName}";
        return avatarUrl;
    }

    public Task DeleteAvatarAsync(string fileUrl, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var filePath = Path.Combine(_options.AvatarPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore deletion errors for now
        }

        return Task.CompletedTask;
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_options.AvatarPath))
        {
            Directory.CreateDirectory(_options.AvatarPath);
        }
    }
}
