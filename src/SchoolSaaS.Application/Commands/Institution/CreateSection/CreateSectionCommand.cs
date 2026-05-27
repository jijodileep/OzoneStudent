using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateSection;

[RequirePermission(PermissionCodes.InstitutionClassesManage)]
public sealed record CreateSectionCommand(
    Guid ClassId,
    string Name,
    int Capacity) : ICommand<CreateSectionResult>;

public sealed record CreateSectionResult(Guid Id, Guid ClassId, string Name, int Capacity);
