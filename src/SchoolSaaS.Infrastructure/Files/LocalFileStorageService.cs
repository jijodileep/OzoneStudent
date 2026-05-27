using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Files;

namespace SchoolSaaS.Infrastructure.Files;

public sealed class LocalFileStorageService(IOptions<FileStorageOptions> options)
    : IFileStorageService, IFileStorageSettings
{
    private readonly FileStorageOptions _options = options.Value;

    public long MaxFileSizeBytes => _options.MaxFileSizeBytes;

    public async Task<string> SaveAsync(
        Guid tenantId,
        string relativePath,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Invalid storage path.");

        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await content.CopyToAsync(fileStream, cancellationToken);
        return relativePath.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Stored file was not found.", storagePath);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var root = Path.GetFullPath(_options.RootPath);
        var fullPath = Path.GetFullPath(Path.Combine(root, normalized));

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path escapes the configured root.");
        }

        return fullPath;
    }
}
