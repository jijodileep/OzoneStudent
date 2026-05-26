using FluentValidation;

namespace SchoolSaaS.Application.Queries.Rbac.ListRoles;

public sealed class ListRolesQueryValidator : AbstractValidator<ListRolesQuery>
{
    public ListRolesQueryValidator()
    {
    }
}
