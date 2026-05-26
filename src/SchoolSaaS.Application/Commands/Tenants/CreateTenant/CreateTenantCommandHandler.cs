using MediatR;
using SchoolSaaS.Application.Abstractions.Platform;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Tenants.CreateTenant;

public sealed class CreateTenantCommandHandler(ITenantOnboardingService onboarding)
    : IRequestHandler<CreateTenantCommand, Result<CreateTenantResult>>
{
    public Task<Result<CreateTenantResult>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken) =>
        onboarding.CreateTenantAsync(request, cancellationToken);
}

