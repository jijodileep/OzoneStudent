using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Api.Middleware;

public sealed class TenantContextMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<TenantContextMiddleware> logger)
{
  private static readonly string[] TenantResolutionSkippedPrefixes =
    [
        "/health",
        "/swagger"
    ];

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (IsTenantResolutionSkipped(context.Request.Path))
        {
            await next(context);
            return;
        }

        var user = context.User;
        tenantContext.IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
        tenantContext.IsSuperAdmin = user.IsInRole("super_admin")
            || user.HasClaim("role", "super_admin");

        tenantContext.UserId = TryParseGuid(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub"));

        tenantContext.TenantId = TryParseGuid(user.FindFirstValue("tenant_id"));

        if (tenantContext.TenantId is null
            && configuration.GetValue<bool>("Tenancy:AllowHeaderTenantId"))
        {
            tenantContext.TenantId = TryParseGuid(
                context.Request.Headers["X-Tenant-Id"].FirstOrDefault());
        }

        // If tenant_id isn't present, resolve tenant by slug (host/subdomain or dev header),
        // then fetch tenant_id from DB. This supports multi-server deployments where tenant config
        // lives entirely in the database.
        if (tenantContext.TenantId is null)
        {
            var slug = ResolveTenantSlug(context, configuration);

            if (!string.IsNullOrWhiteSpace(slug))
            {
                var db = context.RequestServices.GetRequiredService<PlatformDbContext>();
                var tenantId = await db.Tenants
                    .Where(t => t.Slug == slug)
                    .Select(t => (Guid?)t.Id)
                    .SingleOrDefaultAsync();

                tenantContext.TenantId = tenantId;
            }
        }

        tenantContext.BranchId = TryParseGuid(user.FindFirstValue("branch_id"));

        if (tenantContext.IsAuthenticated
            && tenantContext.TenantId is null
            && !tenantContext.IsSuperAdmin)
        {
            logger.LogWarning(
                "Authenticated request missing tenant_id for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                detail = "Tenant context is required.",
                status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        await next(context);
    }

    private static string? ResolveTenantSlug(HttpContext context, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Tenancy:AllowHeaderTenantSlug"))
        {
            var headerSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerSlug))
            {
                return headerSlug.Trim();
            }
        }

        // Basic subdomain parsing: {slug}.example.com -> slug
        var host = context.Request.Host.Host;
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 3)
        {
            return parts[0];
        }

        return null;
    }

    private static bool IsTenantResolutionSkipped(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return TenantResolutionSkippedPrefixes.Any(prefix =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
