using SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListProfileDocuments;

[RequirePermission(PermissionCodes.InstitutionStaffDocumentsManage)]
public sealed record ListStaffDocumentsQuery(Guid StaffId) : IQuery<ListProfileDocumentsResult>;

[RequirePermission(PermissionCodes.StudentsDocumentsManage)]
public sealed record ListStudentDocumentsQuery(Guid StudentId) : IQuery<ListProfileDocumentsResult>;

public sealed record ListProfileDocumentsResult(IReadOnlyList<ProfileDocumentDto> Items);
