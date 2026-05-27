using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateGrade;

[RequirePermission(PermissionCodes.InstitutionClassesManage)]
public sealed record CreateGradeCommand(
    string Name,
    string Code,
    int SortOrder = 0) : ICommand<CreateGradeResult>;

public sealed record CreateGradeResult(Guid Id, string Name, string Code, int SortOrder);
