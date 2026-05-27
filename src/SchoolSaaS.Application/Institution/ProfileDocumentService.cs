using SchoolSaaS.Application.Abstractions.Files;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Institution;

public sealed class ProfileDocumentService(
    ITenantContext tenantContext,
    IProfileDocumentRepository documentRepository,
    IProfileDocumentOwnerValidator ownerValidator,
    IFileStorageService fileStorage,
    IFileStorageSettings storageSettings)
{
    public async Task<Result<ProfileDocumentDto>> UploadAsync(
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        string documentType,
        string fileName,
        string mimeType,
        long fileSizeBytes,
        Stream content,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is null)
        {
            return Result<ProfileDocumentDto>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        if (fileSizeBytes > storageSettings.MaxFileSizeBytes)
        {
            return Result<ProfileDocumentDto>.Failure(
                "documents.file_too_large",
                $"File exceeds maximum size of {storageSettings.MaxFileSizeBytes} bytes.");
        }

        var tenantId = tenantContext.TenantId.Value;
        if (!await ownerValidator.OwnerExistsAsync(tenantId, ownerType, ownerId, cancellationToken))
        {
            var label = ownerType == ProfileDocumentOwnerType.Student ? "Student" : "Staff member";
            return Result<ProfileDocumentDto>.NotFound($"{label} '{ownerId}' was not found.");
        }

        var documentId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(fileName);
        var relativePath =
            $"{tenantId:N}/{ownerType.ToString().ToLowerInvariant()}/{ownerId:N}/{documentId:N}_{safeFileName}";

        await fileStorage.SaveAsync(tenantId, relativePath, content, cancellationToken);

        var document = new ProfileDocument
        {
            Id = documentId,
            TenantId = tenantId,
            OwnerType = ownerType,
            OwnerId = ownerId,
            DocumentType = documentType.Trim(),
            FileName = safeFileName,
            StoragePath = relativePath,
            MimeType = mimeType,
            FileSizeBytes = fileSizeBytes,
            UploadedByUserId = tenantContext.UserId.Value
        };

        await documentRepository.AddAsync(document, cancellationToken);
        return Result<ProfileDocumentDto>.Success(ProfileDocumentMapper.Map(document));
    }

    public async Task<Result> DeleteAsync(
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var document = await documentRepository.GetByIdAsync(tenantId, documentId, cancellationToken);
        if (document is null
            || document.OwnerType != ownerType
            || document.OwnerId != ownerId)
        {
            return Result.NotFound($"Document '{documentId}' was not found.");
        }

        await fileStorage.DeleteAsync(document.StoragePath, cancellationToken);
        await documentRepository.SoftDeleteAsync(document, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ProfileDocumentDownloadResult>> OpenDownloadAsync(
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ProfileDocumentDownloadResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var document = await documentRepository.GetByIdAsync(tenantId, documentId, cancellationToken);
        if (document is null
            || document.OwnerType != ownerType
            || document.OwnerId != ownerId)
        {
            return Result<ProfileDocumentDownloadResult>.NotFound($"Document '{documentId}' was not found.");
        }

        var stream = await fileStorage.OpenReadAsync(document.StoragePath, cancellationToken);
        return Result<ProfileDocumentDownloadResult>.Success(new ProfileDocumentDownloadResult(
            document.FileName,
            document.MimeType,
            stream));
    }
}

public static class ProfileDocumentMapper
{
    public static ProfileDocumentDto Map(ProfileDocument document) =>
        new(
            document.Id,
            document.OwnerType.ToString(),
            document.OwnerId,
            document.DocumentType,
            document.FileName,
            document.MimeType,
            document.FileSizeBytes,
            document.CreatedAt);
}

public sealed record ProfileDocumentDownloadResult(
    string FileName,
    string MimeType,
    Stream Content);
