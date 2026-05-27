using SchoolSaaS.Application.Common;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.DownloadProfileDocument;

[RequirePermission(PermissionCodes.InstitutionStaffDocumentsManage)]
[AuditRead("Institution", nameof(DownloadStaffDocumentQuery))]
public sealed record DownloadStaffDocumentQuery(Guid StaffId, Guid DocumentId) : IQuery<ProfileDocumentDownloadResult>;

[RequirePermission(PermissionCodes.StudentsDocumentsManage)]
[AuditRead("Student", nameof(DownloadStudentDocumentQuery))]
public sealed record DownloadStudentDocumentQuery(Guid StudentId, Guid DocumentId) : IQuery<ProfileDocumentDownloadResult>;
