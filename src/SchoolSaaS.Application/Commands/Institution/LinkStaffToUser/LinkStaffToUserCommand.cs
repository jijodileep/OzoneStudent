using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.LinkStaffToUser;

[RequirePermission(PermissionCodes.InstitutionStaffManage)]
public sealed record LinkStaffToUserCommand(Guid StaffId, Guid UserId) : ICommand<LinkStaffToUserResult>;

public sealed record LinkStaffToUserResult(Guid StaffId, Guid UserId);
