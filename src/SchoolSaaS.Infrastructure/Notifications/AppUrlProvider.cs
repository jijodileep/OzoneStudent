using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Notifications;

namespace SchoolSaaS.Infrastructure.Notifications;

public sealed class AppUrlProvider(IOptions<AppUrlOptions> options) : IAppUrlProvider
{
    public string GetPublicBaseUrl() => options.Value.PublicBaseUrl.TrimEnd('/');
}
