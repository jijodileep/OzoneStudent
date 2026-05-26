using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolSaaS.Api.OpenApi;

public sealed class TenantHeadersOperationFilter(IHostEnvironment environment) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Development only: tenant GUID when JWT tenant_id claim is absent.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Slug",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Development only: tenant slug for host-based resolution.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
