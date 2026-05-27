using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListProfileDocuments;

public sealed class ListStaffDocumentsQueryHandler(
    ITenantContext tenantContext,
    IProfileDocumentRepository repository,
    IProfileDocumentOwnerValidator ownerValidator) : IRequestHandler<ListStaffDocumentsQuery, Result<ListProfileDocumentsResult>>
{
    public async Task<Result<ListProfileDocumentsResult>> Handle(
        ListStaffDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListProfileDocumentsResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        if (!await ownerValidator.OwnerExistsAsync(tenantId, ProfileDocumentOwnerType.Staff, request.StaffId, cancellationToken))
        {
            return Result<ListProfileDocumentsResult>.NotFound($"Staff member '{request.StaffId}' was not found.");
        }

        var documents = await repository.ListAsync(tenantId, ProfileDocumentOwnerType.Staff, request.StaffId, cancellationToken);
        return Result<ListProfileDocumentsResult>.Success(new ListProfileDocumentsResult(
            documents.Select(ProfileDocumentMapper.Map).ToList()));
    }
}

public sealed class ListStudentDocumentsQueryHandler(
    ITenantContext tenantContext,
    IProfileDocumentRepository repository,
    IProfileDocumentOwnerValidator ownerValidator) : IRequestHandler<ListStudentDocumentsQuery, Result<ListProfileDocumentsResult>>
{
    public async Task<Result<ListProfileDocumentsResult>> Handle(
        ListStudentDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListProfileDocumentsResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        if (!await ownerValidator.OwnerExistsAsync(tenantId, ProfileDocumentOwnerType.Student, request.StudentId, cancellationToken))
        {
            return Result<ListProfileDocumentsResult>.NotFound($"Student '{request.StudentId}' was not found.");
        }

        var documents = await repository.ListAsync(tenantId, ProfileDocumentOwnerType.Student, request.StudentId, cancellationToken);
        return Result<ListProfileDocumentsResult>.Success(new ListProfileDocumentsResult(
            documents.Select(ProfileDocumentMapper.Map).ToList()));
    }
}
