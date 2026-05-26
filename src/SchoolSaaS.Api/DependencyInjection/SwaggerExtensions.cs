using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using SchoolSaaS.Api.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolSaaS.Api.DependencyInjection;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"School SaaS {description.GroupName.ToUpperInvariant()}");
            }
        });

        return app;
    }
}
