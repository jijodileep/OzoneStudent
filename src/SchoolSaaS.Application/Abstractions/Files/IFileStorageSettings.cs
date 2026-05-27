namespace SchoolSaaS.Application.Abstractions.Files;

public interface IFileStorageSettings
{
    long MaxFileSizeBytes { get; }
}
