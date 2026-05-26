using FluentValidation;

namespace SchoolSaaS.Application.Queries.Rbac.GetUserPermissions;

public sealed class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
    public GetUserPermissionsQueryValidator()
    {
    }
}
