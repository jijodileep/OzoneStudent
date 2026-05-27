namespace SchoolSaaS.Application.Abstractions.Files;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Guid tenantId,
        string relativePath,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
