using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.DeleteProfileDocument;

[RequirePermission(PermissionCodes.InstitutionStaffDocumentsManage)]
public sealed record DeleteStaffDocumentCommand(Guid StaffId, Guid DocumentId) : ICommand<bool>;

[RequirePermission(PermissionCodes.StudentsDocumentsManage)]
public sealed record DeleteStudentDocumentCommand(Guid StudentId, Guid DocumentId) : ICommand<bool>;
