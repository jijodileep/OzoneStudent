using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenService refreshTokenService,
    ITenantContext tenantContext,
    IAuditService auditService) : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);

        if (tenantContext.UserId is not null)
        {
            await auditService.LogAsync(
                new AuditEntry(
                    AuditActions.Logout,
                    AuditCategories.Auth,
                    EntityType: "User",
                    EntityId: tenantContext.UserId,
                    Description: "User logged out"),
                cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}

