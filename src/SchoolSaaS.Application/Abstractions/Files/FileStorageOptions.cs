namespace SchoolSaaS.Application.Abstractions.Files;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "storage";

    public long MaxFileSizeBytes { get; set; } = 10_485_760;
}
