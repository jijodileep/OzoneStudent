using SchoolSaaS.Application.Commands.Tenants.CreateTenant;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Abstractions.Platform;

public interface ITenantOnboardingService
{
    Task<Result<CreateTenantResult>> CreateTenantAsync(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default);
}
